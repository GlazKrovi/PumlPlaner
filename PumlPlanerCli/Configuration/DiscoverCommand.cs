using Spectre.Console;
using Spectre.Console.Cli;

namespace PumlPlanerCli;

/// <summary>
///     Command to discover PlantUML files in a folder
/// </summary>
public class DiscoverCommand : AsyncCommand<DiscoverCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, DiscoverCommandSettings settings)
    {
        try
        {
            AnsiConsole.MarkupLine($"[bold blue]Discovering PlantUML files in:[/] {settings.FolderPath}");

            if (!Directory.Exists(settings.FolderPath))
            {
                AnsiConsole.MarkupLine($"[bold red]✗[/] Directory not found: {settings.FolderPath}");
                return 1;
            }

            var searchOption = settings.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(settings.FolderPath, settings.Pattern, searchOption);

            if (files.Length == 0)
            {
                AnsiConsole.MarkupLine("[bold yellow]No PlantUML files found.[/]");
                return 0;
            }

            var table = new Table()
                .AddColumn("[bold blue]File[/]")
                .AddColumn("[bold blue]Size[/]")
                .AddColumn("[bold blue]Last Modified[/]");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var relativePath = Path.GetRelativePath(settings.FolderPath, file);
                var size = fileInfo.Length < 1024 ? $"{fileInfo.Length} B" : $"{fileInfo.Length / 1024.0:F1} KB";
                table.AddRow(relativePath, size, fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));
            }

            AnsiConsole.MarkupLine($"[bold green]Found {files.Length} PlantUML files:[/]");
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