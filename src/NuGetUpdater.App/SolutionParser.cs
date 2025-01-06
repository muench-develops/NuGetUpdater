using System.Text.RegularExpressions;
using NuGetUpdater.App.Wrapper;

namespace NuGetUpdater.App;

/// <summary>
/// Parses .sln files to extract project paths.
/// </summary>
internal static class SolutionParser
{
    /// <summary>
    /// Extracts the paths of projects included in a solution file.
    /// </summary>
    /// <param name="solutionPath">The path to the solution file.</param>
    /// <param name="fileSystem">Filesystem Wrapper</param>
    /// <returns>A list of absolute paths to the project files.</returns>
    public static List<string> GetProjectsFromSolution(string solutionPath, IFileSystem fileSystem)
    {
        var projectPaths = new List<string>();

        if (!fileSystem.FileExists(solutionPath))
        {
            throw new FileNotFoundException($"Solution file not found: {solutionPath}");
        }

        string solutionDirectory = fileSystem.GetDirectoryName(solutionPath);
        var projectRegex = new Regex(@"Project\(""\{.*?\}""\) = "".*?"", ""(.*?)"", ""\{.*?\}""");

        foreach (string line in fileSystem.ReadAllLines(solutionPath))
        {
            Match match = projectRegex.Match(line);
            if (!match.Success)
            {
                continue;
            }

            string relativePath = match.Groups[1].Value.Replace("\\", Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
            string fullPath = Path.Combine(solutionDirectory, relativePath);

            if (fileSystem.FileExists(fullPath))
            {
                projectPaths.Add(fullPath);
            }
        }

        return projectPaths;
    }
}
