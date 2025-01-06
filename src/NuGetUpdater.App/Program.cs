using NuGetUpdater.App;
using NuGetUpdater.App.Wrapper;
using Serilog;

ConsoleRenderer.DisplayHeader("NuGet Updater");

var fileSystem = new FileSystem();
var restoreService = new RestoreService();
var updateService = new UpdateService(fileSystem);

if (ValidateInput(args))
{
    return;
}

string? projectPath = CommandLineParser.GetFlagValue(args, "--path");
string? updateType = CommandLineParser.GetFlag(args, ["--major", "--minor", "--patch"]);
string? logPath = CommandLineParser.GetFlagValue(args, "--log");

AddLogging(logPath);

if (string.IsNullOrEmpty(updateType))
{
    ConsoleRenderer.DisplayError("One of --major, --minor, or --patch flags must be specified.");
    return;
}

// Determine project or solution files
List<string> projectFiles = GetProjectFiles(projectPath);

await ProcessProjectFilesAsync(projectFiles, updateType);

return;

void AddLogging(string? s)
{
    if (!string.IsNullOrEmpty(s))
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(formatProvider: System.Globalization.CultureInfo.InvariantCulture)
            .WriteTo.File(s, rollingInterval: RollingInterval.Day, formatProvider: System.Globalization.CultureInfo.InvariantCulture)
            .CreateLogger();

        Log.Information("Logging initialized. Log file: {LogPath}", s);
    }
    else
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(formatProvider: System.Globalization.CultureInfo.InvariantCulture)
            .CreateLogger();

        Log.Information("Logging initialized without a log file.");
    }
}

bool ValidateInput(string[] strings)
{
    // Parse command-line arguments
    (bool isValid, string errorMessage) = CommandLineParser.ValidateArguments(strings);
    if (isValid)
    {
        return false;
    }

    ConsoleRenderer.DisplayError(errorMessage);
    return true;

}

List<string> GetProjectFiles(string? projectPath1)
{
    List<string> list = File.Exists(projectPath1) && projectPath1.EndsWith(".sln", StringComparison.Ordinal)
        ? [.. SolutionParser.GetProjectsFromSolution(projectPath1, new FileSystem())]
        : [projectPath1 ?? string.Empty];
    return list;
}

async Task ProcessProjectFilesAsync(List<string> projectFiles1, string updateType1)
{
    foreach (string projectFile in projectFiles1)
    {
        // Log project processing
        Log.Information("Processing project: {ProjectFile}", Path.GetFileName(projectFile));

        bool anyUpdatedDependencies = await updateService.UpdateDependenciesAsync(projectFile, updateType1);

        // Run restore
        if (anyUpdatedDependencies)
        {
            restoreService.Restore(projectFile);
        }

        Log.Information("Project {ProjectFile} updated successfully.", Path.GetFileName(projectFile));
        ConsoleRenderer.DisplaySuccess($"Project {Path.GetFileName(projectFile)} updated successfully.");
    }
}
