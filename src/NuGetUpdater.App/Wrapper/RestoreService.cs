using System.Diagnostics;
using System.Xml.Linq;
using Serilog;

namespace NuGetUpdater.App.Wrapper;

internal class RestoreService : IRestoreService
{
    /// <summary>
    /// Restores the specified project by determining if it is a .NET Core project or not.
    /// </summary>
    /// <param name="projectFilePath">The file path of the project to restore.</param>
    public void Restore(string projectFilePath)
    {
        bool isCoreProject = IsDotNetCoreProject(projectFilePath);
        if (isCoreProject)
        {
            Log.Information("Restoring project {ProjectFilePath} using dotnet", projectFilePath);
            ExecuteCommand("dotnet", $"restore \"{projectFilePath}\"");
        }
        else
        {
            Log.Information("Restoring project {ProjectFilePath} using msbuild", projectFilePath);
            ExecuteCommand("msbuild", $"\"{projectFilePath}\" /restore");
        }
    }

    internal static bool IsDotNetCoreProject(string projectFilePath)
    {
        var doc = XDocument.Load(projectFilePath);
        string? targetFramework = doc.Descendants("TargetFramework").FirstOrDefault()?.Value;
        return !string.IsNullOrEmpty(targetFramework) &&
               (targetFramework.StartsWith("netcoreapp", StringComparison.Ordinal) || targetFramework.StartsWith("netstandard", StringComparison.Ordinal) || targetFramework.StartsWith("net", StringComparison.Ordinal));
    }

    private static void ExecuteCommand(string fileName, string arguments)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(processInfo);
            process?.WaitForExit();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing command {FileName} {Arguments}", fileName, arguments);
        }
    }
}
