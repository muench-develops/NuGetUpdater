using System.Xml.Linq;
using NuGetUpdater.App.Wrapper;

namespace NuGetUpdater.App;

/// <summary>
/// Loads dependencies from project files (.csproj, packages.config) or solutions (.sln).
/// </summary>
internal static class DependencyLoader
{
    /// <summary>
    /// Loads dependencies from a given project or solution file.
    /// </summary>
    /// <param name="filePath">The path to the project (.csproj) or solution (.sln) file.</param>
    /// <returns>A list of dependencies with their package IDs and versions.</returns>
    public static List<Dependency> LoadDependencies(string filePath)
    {
        if (filePath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
        {
            return LoadFromSolution(filePath);
        }
        else if (filePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            return LoadFromProject(filePath);
        }
        else
        {
            throw new NotSupportedException($"Unsupported file type: {filePath}");
        }
    }

    private static List<Dependency> LoadFromSolution(string slnPath)
    {
        var dependencies = new List<Dependency>();
        List<string>? projects = SolutionParser.GetProjectsFromSolution(slnPath, new FileSystem());

        foreach (string project in projects)
        {
            dependencies.AddRange(LoadFromProject(project));
        }

        return dependencies;
    }

    private static List<Dependency> LoadFromProject(string csprojPath)
    {
        var dependencies = new List<Dependency>();

        // Check if a packages.config exists alongside the project
        string? projectDirectory = Path.GetDirectoryName(csprojPath) ?? throw new InvalidOperationException($"Error loading dependencies from {csprojPath}: could not determine project directory.");

        string packagesConfigPath = Path.Combine(projectDirectory, "packages.config");

        dependencies.AddRange(File.Exists(packagesConfigPath)
            ? LoadFromPackagesConfig(packagesConfigPath)
            : LoadFromCsproj(csprojPath));

        return dependencies;
    }

    private static List<Dependency> LoadFromCsproj(string csprojPath)
    {
        var dependencies = new List<Dependency>();
        try
        {
            var doc = XDocument.Load(csprojPath);

            IEnumerable<XElement> packageReferences = doc.Descendants("PackageReference");
            foreach (XElement reference in packageReferences)
            {
                string? packageId = reference.Attribute("Include")?.Value;
                string? version = reference.Attribute("Version")?.Value;

                if (!string.IsNullOrEmpty(packageId) && !string.IsNullOrEmpty(version))
                {
                    dependencies.Add(new Dependency(packageId, version));
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error loading dependencies from {csprojPath}: {ex.Message}");
        }

        return dependencies;
    }

    private static List<Dependency> LoadFromPackagesConfig(string configPath)
    {
        var dependencies = new List<Dependency>();
        try
        {
            var doc = XDocument.Load(configPath);

            IEnumerable<XElement> packages = doc.Descendants("package");
            foreach (XElement package in packages)
            {
                string? packageId = package.Attribute("id")?.Value;
                string? version = package.Attribute("version")?.Value;

                if (!string.IsNullOrEmpty(packageId) && !string.IsNullOrEmpty(version))
                {
                    dependencies.Add(new Dependency(packageId, version));
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error loading dependencies from {configPath}: {ex.Message}");
        }

        return dependencies;
    }
}

/// <summary>
/// Represents a NuGet dependency with package ID and version.
/// </summary>
internal record Dependency(string PackageId, string Version);
