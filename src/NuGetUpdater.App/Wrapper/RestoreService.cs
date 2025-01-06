using System.Diagnostics;
using System.Xml.Linq;
using Serilog;

namespace NuGetUpdater.App.Wrapper;

public class RestoreService : IRestoreService
{
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
               (targetFramework.StartsWith("netcoreapp") || targetFramework.StartsWith("netstandard") || targetFramework.StartsWith("net"));
    }

    private static void ExecuteCommand(string fileName, string arguments)
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
}
