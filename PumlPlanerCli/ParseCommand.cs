using Spectre.Console;
using Spectre.Console.Cli;

namespace PumlPlanerCli;

/// <summary>
///     Command to parse a PlantUML file
/// </summary>
public class ParseCommand : AsyncCommand<ParseCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ParseCommandSettings settings)
    {
        try
        {
            AnsiConsole.Status()
                .Start("Parsing PlantUML file...", ctx =>
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.Status($"Reading {settings.FilePath}");
                });

            // Check if file exists
            if (!File.Exists(settings.FilePath))
            {
                AnsiConsole.MarkupLine($"[bold red]✗[/] File not found: {settings.FilePath}");
                return 1;
            }

            // Simulate parsing work
            await Task.Delay(1000);

            AnsiConsole.MarkupLine("[bold green]✓[/] File parsed successfully!");

            if (!settings.Verbose) return 0;
            var fileInfo = new FileInfo(settings.FilePath);
            var panel = new Panel(
                $"[bold blue]File:[/] {settings.FilePath}\n" +
                $"[bold blue]Size:[/] {fileInfo.Length} bytes\n" +
                $"[bold blue]Last Modified:[/] {fileInfo.LastWriteTime}"
            )
            {
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 1)
            };
            AnsiConsole.Write(panel);

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Error:[/] {ex.Message}");
            return 1;
        }
    }
}