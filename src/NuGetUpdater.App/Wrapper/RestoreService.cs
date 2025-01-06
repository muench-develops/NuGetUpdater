using System.Diagnostics;
using System.Xml.Linq;

namespace NuGetUpdater.App.Wrapper;

public class RestoreService : IRestoreService
{
    public void Restore(string projectFilePath)
    {
        bool isCoreProject = IsDotNetCoreProject(projectFilePath);
        if (isCoreProject)
        {
            ExecuteCommand("dotnet", $"restore \"{projectFilePath}\"");
        }
        else
        {
            ExecuteCommand("msbuild", $"\"{projectFilePath}\" /restore");
        }
    }

    private static bool IsDotNetCoreProject(string projectFilePath)
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
