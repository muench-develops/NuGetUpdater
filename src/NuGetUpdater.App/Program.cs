
// Display the header

using NuGet.Versioning;
using NuGetUpdater.App;
using Serilog;

ConsoleRenderer.DisplayHeader("NuGet Updater");

// Parse command-line arguments
(bool IsValid, string ErrorMessage) = CommandLineParser.ValidateArguments(args);
if (!IsValid)
{
    ConsoleRenderer.DisplayError(ErrorMessage);
    return;
}

string? projectPath = CommandLineParser.GetFlagValue(args, "--path");
string? updateType = CommandLineParser.GetFlag(args, ["--major", "--minor", "--patch"]);
string? logPath = CommandLineParser.GetFlagValue(args, "--log");

if (!string.IsNullOrEmpty(logPath))
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console(formatProvider: System.Globalization.CultureInfo.InvariantCulture)
        .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, formatProvider: System.Globalization.CultureInfo.InvariantCulture)
        .CreateLogger();

    Log.Information("Logging initialized. Log file: {LogPath}", logPath);
}
else
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console(formatProvider: System.Globalization.CultureInfo.InvariantCulture)
        .CreateLogger();

    Log.Information("Logging initialized without a log file.");
}

if (string.IsNullOrEmpty(updateType))
{
    ConsoleRenderer.DisplayError("One of --major, --minor, or --patch flags must be specified.");
    return;
}

// Determine project or solution files
List<string> projectFiles = File.Exists(projectPath) && projectPath.EndsWith(".sln", StringComparison.Ordinal)
    ? [.. SolutionParser.GetProjectsFromSolution(projectPath)]
    : [projectPath ?? string.Empty];

foreach (string projectFile in projectFiles)
{
    // Log project processing
    Log.Information("Processing project: {ProjectFile}", Path.GetFileName(projectFile));

    // Load dependencies for the project
    List<Dependency> dependencies = DependencyLoader.LoadDependencies(projectFile);

    // Fetch the latest versions for each dependency
    var dependenciesWithUpdates = new List<(Dependency Current, NuGetVersion? Latest)>();
    foreach (Dependency dependency in dependencies)
    {
        var currentVersion = new NuGetVersion(dependency.Version);
        NuGetVersion? latestVersion = await Nugetupdate.GetLatestVersion(dependency.PackageId, updateType, currentVersion);

        Log.Information("Checking package {PackageId}: Current: {CurrentVersion}, Latest: {LatestVersion}", dependency.PackageId, dependency.Version, latestVersion);

        // Skip packages that are already up-to-date
        if (latestVersion == null || dependency.Version == latestVersion.ToString())
        {
            Log.Information("Package {PackageId} is already up-to-date.", dependency.PackageId);
            continue;
        }

        dependenciesWithUpdates.Add((dependency, latestVersion));
    }

    // Display project and its packages with updates
    ConsoleRenderer.DisplayProjectWithPackages(Path.GetFileName(projectFile), dependenciesWithUpdates, updateType);
}

Console.ReadKey();
