using System.Reflection;
using Spectre.Console;

AnsiConsole.MarkupLineInterpolated($"[blue]File Deduplicator[/]");

var version = Assembly
    .GetExecutingAssembly()
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
    ?.InformationalVersion;

AnsiConsole.MarkupLineInterpolated($"[grey]Version: {version ?? "N/A"}[/]");

const string currentDirectoryOption = "Current Directory";
const string differentDirectoryOption = "Different Directory";

var directoryOption = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title(@"
Search [blue]current[/] or [blue]different[/] directory?
Use ↑ and ↓ to change selection and hit [green]<Enter>[/] to confirm."
        )
        .AddChoices(currentDirectoryOption, differentDirectoryOption)
);

var directory = directoryOption switch
{
    currentDirectoryOption => Environment.CurrentDirectory,
    differentDirectoryOption => AnsiConsole.Ask<string>("Enter directory to search:"),
    _ => throw new InvalidOperationException(nameof(directoryOption)),
};

// Windows interprets paths that end with ":" to mean "switch to the last folder the user visited
// on this drive." instead of "switch to the root directory of this drive" like the user might expect.
// This appends "\" to paths that end with ":" to search from the root directory.
if (directory.EndsWith(':'))
{
    directory = $"{directory}\\";
}

AnsiConsole.MarkupLineInterpolated($"[green]Searching[/] {directory}");

var stopwatch = new System.Diagnostics.Stopwatch();
stopwatch.Start();

var files = Directory.GetFiles(
    path: directory,
    searchPattern: "*",
    enumerationOptions: new EnumerationOptions()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true,
    });

var extensions = files
    .Select(f => Path.GetExtension(f))
    .GroupBy(e => e)
    .OrderByDescending(g => g.Count())
    .Select(g => new { Extension = g.First(), Count = g.Count() });

var table = new Table()
    .AddColumns("Extension", "Count")
    .RoundedBorder()
    .BorderColor(Color.Grey)
    .Title("[green]Distinct File Extensions[/]");

foreach (var extension in extensions)
{
    table.AddRow(
        string.IsNullOrEmpty(extension.Extension) ? "(no extension)" : extension.Extension,
        extension.Count.ToString("n0"));
}

AnsiConsole.Write(table);

AnsiConsole.WriteLine();

stopwatch.Stop();
var searchTime = stopwatch.ElapsedMilliseconds > 10_000
    ? stopwatch.ElapsedMilliseconds / 1000
    : stopwatch.ElapsedMilliseconds;
var searchTimeUnit = stopwatch.ElapsedMilliseconds > 10_000
    ? "s"
    : "ms";

AnsiConsole.MarkupLineInterpolated($"[grey]Loaded {extensions.Count()} extensions in {searchTime}{searchTimeUnit}[/]");