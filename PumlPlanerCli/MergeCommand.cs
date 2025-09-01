using Spectre.Console;
using Spectre.Console.Cli;

namespace PumlPlanerCli;

/// <summary>
///     Command to merge multiple schemas
/// </summary>
public class MergeCommand : AsyncCommand<MergeCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, MergeCommandSettings settings)
    {
        try
        {
            AnsiConsole.MarkupLine($"[bold blue]Merging {settings.SchemaFiles.Length} schemas...[/]");

            // Check if all files exist
            var missingFiles = settings.SchemaFiles.Where(f => !File.Exists(f)).ToArray();
            if (missingFiles.Any())
            {
                AnsiConsole.MarkupLine("[bold red]✗[/] Some files not found:");
                foreach (var file in missingFiles) AnsiConsole.MarkupLine($"  - {file}");
                return 1;
            }

            // Simulate merging logic
            await Task.Delay(1200);

            AnsiConsole.MarkupLine("[bold green]✓[/] Schemas merged successfully!");

            var panel = new Panel(
                $"[bold blue]Output File:[/] {settings.OutputFile}\n" +
                $"[bold blue]Format:[/] {settings.Format}\n" +
                $"[bold blue]Input Schemas:[/] {string.Join(", ", settings.SchemaFiles)}"
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