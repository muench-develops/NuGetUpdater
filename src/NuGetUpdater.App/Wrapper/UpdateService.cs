using NuGet.Versioning;
using Serilog;

namespace NuGetUpdater.App.Wrapper;

internal class UpdateService(IFileSystem fileSystem) : IUpdateService
{
    /// <summary>
    /// Updates the dependencies of a given project to their latest versions based on the specified update type.
    /// </summary>
    /// <param name="projectPath">The file path to the project whose dependencies need to be updated.</param>
    /// <param name="updateType">The type of update to perform (e.g., major, minor, patch).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether any dependencies were updated.</returns>
    public async Task<bool> UpdateDependenciesAsync(string projectPath, string updateType)
    {
        List<Dependency> dependencies = DependencyLoader.LoadDependencies(projectPath);
        var dependenciesWithUpdates = new List<(Dependency Current, NuGetVersion? Latest)>();

        foreach (Dependency dependency in dependencies)
        {
            var currentVersion = new NuGetVersion(dependency.Version);
            NuGetVersion? latestVersion =
                await Nugetupdate.GetLatestVersion(dependency.PackageId, updateType, currentVersion);

            if (latestVersion == null || currentVersion >= latestVersion)
            {
                continue;
            }

            UpdatePackageVersion(projectPath, dependency.PackageId, latestVersion.ToString());
            dependenciesWithUpdates.Add((dependency, latestVersion));
        }

        ConsoleRenderer.DisplayProjectWithPackages(Path.GetFileName(projectPath), dependenciesWithUpdates);

        return dependenciesWithUpdates.Count > 0;
    }

    private void UpdatePackageVersion(string projectPath, string packageId, string newVersion)
    {
        if (RestoreService.IsDotNetCoreProject(projectPath))
        {
            Nugetupdate.UpdatePackageVersionInCsproj(projectPath, packageId, newVersion);
            return;
        }

        string configPath = Path.Combine(fileSystem.GetDirectoryName(projectPath), "packages.config");
        Log.Information("Updating {PackageId} to {NewVersion} in {ConfigPath}", packageId, newVersion, configPath);
        Nugetupdate.UpdatePackageVersionInPackagesConfig(configPath, packageId, newVersion);
    }
}
