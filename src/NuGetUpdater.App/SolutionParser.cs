using System.Text.RegularExpressions;

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
    /// <returns>A list of absolute paths to the project files.</returns>
    public static List<string> GetProjectsFromSolution(string solutionPath)
    {
        var projectPaths = new List<string>();

        try
        {
            string? solutionDirectory = Path.GetDirectoryName(solutionPath);
            if (solutionDirectory == null || !File.Exists(solutionPath))
            {
                throw new FileNotFoundException($"Solution file not found: {solutionPath}");
            }

            // Regex to match .csproj entries in the .sln file
            var projectRegex = new Regex(@"Project\(""\{.*?\}""\) = "".*?"", ""(.*?)"", ""\{.*?\}""");

            foreach (string line in File.ReadAllLines(solutionPath))
            {
                Match match = projectRegex.Match(line);
                if (!match.Success)
                {
                    continue;
                }

                // Convert relative path to absolute path
                string relativePath = match.Groups[1].Value.Replace("\\", Path.DirectorySeparatorChar.ToString());
                string fullPath = Path.Combine(solutionDirectory, relativePath);

                if (File.Exists(fullPath))
                {
                    projectPaths.Add(fullPath);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error parsing solution file '{solutionPath}': {ex.Message}");
        }

        return projectPaths;
    }
}
