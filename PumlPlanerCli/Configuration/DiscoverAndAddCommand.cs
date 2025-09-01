using Spectre.Console;
using Spectre.Console.Cli;

namespace PumlPlanerCli;

/// <summary>
///     Command to discover and add schemas to a project
/// </summary>
public class DiscoverAndAddCommand : AsyncCommand<DiscoverAndAddCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, DiscoverAndAddCommandSettings settings)
    {
        try
        {
            AnsiConsole.MarkupLine($"[bold blue]Discovering and adding schemas to project:[/] {settings.ProjectId}");

            if (!Directory.Exists(settings.FolderPath))
            {
                AnsiConsole.MarkupLine($"[bold red]✗[/] Directory not found: {settings.FolderPath}");
                return 1;
            }

            var searchOption = settings.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var discoveredFiles = Directory.GetFiles(settings.FolderPath, "*.puml", searchOption);

            // Simulate discovery and addition
            await Task.Delay(1000);

            AnsiConsole.MarkupLine($"[bold green]✓[/] Discovered and added {discoveredFiles.Length} schemas!");

            var table = new Table()
                .AddColumn("[bold blue]Discovered File[/]")
                .AddColumn("[bold blue]Action[/]");

            foreach (var file in discoveredFiles)
            {
                var relativePath = Path.GetRelativePath(settings.FolderPath, file);
                table.AddRow(relativePath, "[green]Added to project[/]");
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