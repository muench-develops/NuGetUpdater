using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Serilog;

namespace NuGetUpdater.App;

/// <summary>
/// Handles NuGet operations like fetching the latest package versions.
/// </summary>
internal static class Nugetupdate
{
    private static readonly string? _nuGetFeedUrl = GetNuGetFeedUrl();

    private static string? GetNuGetFeedUrl()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        return configuration["NuGetSettings:FeedUrl"];
    }

    /// <summary>
    /// Retrieves the latest version of a NuGet package based on the update type.
    /// </summary>
    /// <param name="packageId">The ID of the package.</param>
    /// <param name="updateType">The type of update (--major, --minor, --patch).</param>
    /// <returns>The latest version that matches the update type or null if none found.</returns>
    public static async Task<NuGetVersion?> GetLatestVersion(string packageId, string updateType,
        NuGetVersion currentVersion)
    {
        try
        {
            SourceRepository repository = Repository.Factory.GetCoreV3(_nuGetFeedUrl);
            FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

            using var cache = new SourceCacheContext();
            IEnumerable<NuGetVersion> versions =
                await resource.GetAllVersionsAsync(packageId, cache, NullLogger.Instance, CancellationToken.None);

            Log.Debug("Available versions for {PackageId}: {Versions}", packageId, string.Join(", ", versions));

            // Filter valid target versions
            var validVersions = versions.Where(v => VersionFilter.IsValidUpdate(v, updateType, currentVersion))
                .ToList();

            Log.Debug("Valid versions for {PackageId} ({UpdateType}): {ValidVersions}",
                packageId, updateType, string.Join(", ", validVersions));

            return validVersions.OrderByDescending(v => v).FirstOrDefault();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching latest version for package: {PackageId}", packageId);
            return null;
        }
    }

    public static void UpdatePackageVersionInPackagesConfig(string configPath, string packageId, string newVersion)
    {
        try
        {
            var doc = XDocument.Load(configPath);
            Log.Debug("Loaded packages.config from {ConfigPath}", configPath);

            // Suche nach dem Paket
            XElement? packageElement = doc.Descendants("package")
                .FirstOrDefault(pkg => pkg.Attribute("id")?.Value == packageId);

            Log.Debug("Found package {PackageId} in {ConfigPath}: {PackageElement}", packageId, configPath,
                packageElement);

            if (packageElement == null)
            {
                Log.Warning("Package {PackageId} not found in packages.config {ConfigPath}.", packageId, configPath);
                return;
            }

            // Aktualisiere die Version
            packageElement.SetAttributeValue("version", newVersion);
            doc.Save(configPath);

            Log.Information("Updated {PackageId} to version {NewVersion} in {ConfigPath}.", packageId, newVersion,
                configPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating package {PackageId} in {ConfigPath}.", packageId, configPath);
        }
    }

    public static void UpdatePackageVersionInCsproj(string projectFilePath, string packageId, string newVersion)
    {
        try
        {
            var doc = XDocument.Load(projectFilePath);

            // Suche nach dem PackageReference
            XElement? packageReference = doc.Descendants("PackageReference")
                .FirstOrDefault(pr => pr.Attribute("Include")?.Value == packageId);

            if (packageReference == null)
            {
                Log.Warning("Package {PackageId} not found in project file {ProjectFilePath}.", packageId,
                    projectFilePath);
                return;
            }

            // Aktualisiere die Version
            packageReference.SetAttributeValue("Version", newVersion);
            doc.Save(projectFilePath);

            Log.Information("Updated {PackageId} to version {NewVersion} in {ProjectFilePath}.", packageId, newVersion,
                projectFilePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating package {PackageId} in {ProjectFilePath}.", packageId, projectFilePath);
        }
    }
}

/// <summary>
/// Filters package versions based on the desired update type.
/// </summary>
internal static class VersionFilter
{
    /// <summary>
    /// Checks if the given version is a valid update based on the update type.
    /// </summary>
    /// <param name="version">The version to check.</param>
    /// <param name="updateType">The type of update (--major, --minor, --patch).</param>
    /// <param name="currentVersion">The current version of the package.</param>
    /// <param name="includePrerelease">Include prerelease versions if true.</param>
    /// <returns>True if the version is a valid update, otherwise false.</returns>
    public static bool IsValidUpdate(NuGetVersion version, string updateType, NuGetVersion currentVersion,
        bool includePrerelease = false)
    {
        // Ignore prerelease versions unless explicitly allowed
        if (version.IsPrerelease && !includePrerelease)
        {
            return false;
        }

        return updateType switch
        {
            "--major" => version.Major > currentVersion.Major, // Major version increment
            "--minor" => version.Major == currentVersion.Major &&
                         version.Minor > currentVersion.Minor, // Same major, minor increment
            "--patch" => version.Major == currentVersion.Major && version.Minor == currentVersion.Minor &&
                         version.Patch > currentVersion.Patch, // Same major & minor, patch increment
            _ => false
        };
    }
}
