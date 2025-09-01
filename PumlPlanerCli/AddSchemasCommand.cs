using Spectre.Console;
using Spectre.Console.Cli;

namespace PumlPlanerCli;

/// <summary>
///     Command to add schemas to a project
/// </summary>
public class AddSchemasCommand : AsyncCommand<AddSchemasCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, AddSchemasCommandSettings settings)
    {
        try
        {
            AnsiConsole.MarkupLine($"[bold blue]Adding schemas to project:[/] {settings.ProjectId}");

            // Simulate schema addition
            await Task.Delay(600);

            AnsiConsole.MarkupLine($"[bold green]✓[/] Added {settings.SchemaFiles.Length} schemas to project!");

            var table = new Table()
                .AddColumn("[bold blue]Schema File[/]")
                .AddColumn("[bold blue]Status[/]");

            foreach (var file in settings.SchemaFiles)
            {
                var exists = File.Exists(file);
                var status = exists ? "[green]Added[/]" : "[red]File not found[/]";
                table.AddRow(file, status);
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