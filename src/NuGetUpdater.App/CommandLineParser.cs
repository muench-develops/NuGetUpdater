namespace NuGetUpdater.App;

/// <summary>
/// Handles parsing and validation of command-line arguments.
/// </summary>
public static class CommandLineParser
{
    /// <summary>
    /// Retrieves the value of a specific flag.
    /// </summary>
    /// <param name="args">The array of command-line arguments.</param>
    /// <param name="flag">The flag to retrieve the value for.</param>
    /// <returns>The value of the flag or null if not found.</returns>
    public static string? GetFlagValue(string?[] args, string flag)
    {
        int index = Array.IndexOf(args, flag);
        return index >= 0 && index < args.Length - 1 ? args[index + 1]! : null;
    }

    /// <summary>
    /// Checks if a specific flag is present in the arguments.
    /// </summary>
    /// <param name="args">The array of command-line arguments.</param>
    /// <param name="flags">A list of possible flags to check.</param>
    /// <returns>The matched flag or null if none found.</returns>
    public static string? GetFlag(string?[] args, string[] flags) => flags.FirstOrDefault(args.Contains);

    /// <summary>
    /// Validates the required arguments for the tool.
    /// </summary>
    /// <param name="args">The array of command-line arguments.</param>
    /// <returns>A tuple indicating whether validation succeeded and an error message if not.</returns>
    public static (bool IsValid, string ErrorMessage) ValidateArguments(string?[] args)
    {
        // Ensure path is provided
        string? path = GetFlagValue(args, "--path");
        if (string.IsNullOrEmpty(path))
        {
            return (false, "The --path flag is required and must specify a valid project or solution path.");
        }

        // Ensure one of the update types is specified
        string? updateType = GetFlag(args, ["--major", "--minor", "--patch"]);
        if (string.IsNullOrEmpty(updateType))
        {
            return (false, "One of --major, --minor, or --patch flags must be specified.");
        }

        // Path must exist
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            return (false, $"The specified path '{path}' does not exist.");
        }

        return (true, string.Empty);
    }
}
