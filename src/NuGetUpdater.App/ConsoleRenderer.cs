using NuGet.Versioning;
using Spectre.Console;

namespace NuGetUpdater.App;

/// <summary>
/// Handles rendering of console output using Spectre.Console.
/// </summary>
internal static class ConsoleRenderer
{
    /// <summary>
    /// Displays the header with a given title.
    /// </summary>
    /// <param name="title">The title to display.</param>
    public static void DisplayHeader(string title) =>
        AnsiConsole.Write(
            new FigletText(title)
                .Centered()
                .Color(Color.Green));

    /// <summary>
    /// Displays a project and its associated packages in a grouped table format.
    /// Highlights major updates in red.
    /// </summary>
    /// <param name="projectFileName">The name of the project file being processed.</param>
    /// <param name="dependencies">A list of dependencies for the project.</param>
    /// <param name="updateType">The type of update (--major, --minor, --patch).</param>
    public static void DisplayProjectWithPackages(string projectFileName, List<(Dependency Current, NuGetVersion? Latest)> dependencies)
    {
        Table table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold yellow]Project[/]")
            .AddColumn("[bold yellow]Packages[/]");

        if (dependencies.Count > 0)
        {
            IEnumerable<string> packageDetails = dependencies.Select(d =>
            {
                string currentVersion = d.Current.Version;
                string latestVersion = d.Latest?.ToString() ?? "N/A";

                bool isMajorUpdate = d.Latest != null && d.Latest.Major > new NuGetVersion(currentVersion).Major;

                string packageName = isMajorUpdate
                    ? $"[bold red]{d.Current.PackageId}[/]"
                    : $"[dim]{d.Current.PackageId}[/]";
                string versionInfo = isMajorUpdate
                    ? $"[bold red]{currentVersion} → {latestVersion}[/]"
                    : $"[green]{currentVersion} → {latestVersion}[/]";

                return $"{packageName}\n{versionInfo}";
            });

            _ = table.AddRow(
                $"[bold]{projectFileName}[/]",
                string.Join("\n\n", packageDetails)
            );
        }
        else
        {
            _ = table.AddRow($"[bold]{projectFileName}[/]", "[italic dim]No updates available[/]");
        }

        AnsiConsole.Write(table);
    }
    /// <summary>
    /// Displays an error message in red.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    public static void DisplayError(string message) =>
        AnsiConsole.MarkupLine($"[bold red]Error:[/] {Markup.Escape(message)}");

    /// <summary>
    /// Displays a success message in green.
    /// </summary>
    /// <param name="message">The success message to display.</param>
    public static void DisplaySuccess(string message) =>
        AnsiConsole.MarkupLine($"[bold green]Success:[/] {message}");

    /// <summary>
    /// Displays the result of an update process for a package.
    /// </summary>
    /// <param name="packageId">The ID of the package.</param>
    /// <param name="currentVersion">The current version of the package.</param>
    /// <param name="latestVersion">The latest version of the package (if available).</param>
    /// <param name="dryRun">Indicates if this is a dry-run.</param>
    public static void DisplayUpdateResult(string packageId, string currentVersion,
        NuGet.Versioning.NuGetVersion latestVersion, bool dryRun)
    {
        string action = dryRun ? "[italic yellow]Would update[/]" : "[green]Updated[/]";
        AnsiConsole.MarkupLine($"{action} {packageId} from {currentVersion} to {latestVersion}");
    }

    /// <summary>
    /// Displays a horizontal rule to separate sections.
    /// </summary>
    public static void DisplayDivider() => AnsiConsole.Write(new Rule().RuleStyle("green"));

    /// <summary>
    /// Display a normal message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public static void DisplayMessage(string message) => AnsiConsole.MarkupLine(message);
}
