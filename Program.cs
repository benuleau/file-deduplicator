using Spectre.Console;

const string currentDirectoryOption = "Current Directory";
const string differentDirectoryOption = "Different Directory";

var directoryOption = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Search [green]current[/] or [yellow]different[/] directory?")
        .AddChoices(currentDirectoryOption, differentDirectoryOption)
);

var directory = directoryOption switch
{
    currentDirectoryOption => Environment.CurrentDirectory,
    differentDirectoryOption => AnsiConsole.Ask<string>("Enter directory to search:"),
    _ => throw new InvalidOperationException(nameof(directoryOption)),
};

AnsiConsole.MarkupLineInterpolated($"[green]Searching[/] {directory}");

var stopwatch = new System.Diagnostics.Stopwatch();
stopwatch.Start();

var files = Directory.GetFiles(
    path: directory,
    searchPattern: "*",
    searchOption: SearchOption.AllDirectories);

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
        extension.Count.ToString());
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