using Spectre.Console;
using Spectre.Console.Cli;

namespace PumlPlanerCli;

/// <summary>
///     Command to generate output files
/// </summary>
public class GenerateCommand : AsyncCommand<GenerateCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateCommandSettings settings)
    {
        try
        {
            AnsiConsole.MarkupLine($"[bold blue]Generating outputs for project:[/] {settings.ProjectId}");

            // Simulate generation logic
            await Task.Delay(1500);

            AnsiConsole.MarkupLine("[bold green]✓[/] Output files generated successfully!");

            var table = new Table()
                .AddColumn("[bold blue]Format[/]")
                .AddColumn("[bold blue]Output File[/]")
                .AddColumn("[bold blue]Size[/]");

            foreach (var format in settings.Formats)
            {
                var outputFile = $"{settings.ProjectId}.{format}";
                var size = $"{Random.Shared.Next(10, 100)} KB";
                table.AddRow(format, outputFile, size);
            }

            AnsiConsole.Write(table);

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Error:[/] {ex.Message}");
            return 1;
        }
    }
}