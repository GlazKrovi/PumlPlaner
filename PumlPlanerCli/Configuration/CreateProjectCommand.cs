using Spectre.Console;
using Spectre.Console.Cli;

namespace PumlPlanerCli;

/// <summary>
///     Command to create a new project
/// </summary>
public class CreateProjectCommand : AsyncCommand<CreateProjectCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, CreateProjectCommandSettings settings)
    {
        try
        {
            AnsiConsole.MarkupLine($"[bold blue]Creating project:[/] {settings.ProjectName}");

            // Simulate project creation
            await Task.Delay(800);

            var projectId = Guid.NewGuid().ToString("N")[..8];

            AnsiConsole.MarkupLine("[bold green]✓[/] Project created successfully!");

            var panel = new Panel(
                $"[bold blue]Project ID:[/] {projectId}\n" +
                $"[bold blue]Name:[/] {settings.ProjectName}\n" +
                $"[bold blue]Description:[/] {settings.Description}\n" +
                $"[bold blue]Output Path:[/] {settings.OutputPath}"
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