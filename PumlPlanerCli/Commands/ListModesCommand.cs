using Spectre.Console;
using Spectre.Console.Cli;
using PumlSchemasManager.Core;

namespace PumlPlanerCli.Commands;

/// <summary>
/// Settings for list-modes command
/// </summary>
public class ListModesCommandSettings : CommandSettings
{
    [CommandOption("--verbose|-v")]
    public bool Verbose { get; set; }
}

/// <summary>
/// Command to list available parsing modes
/// </summary>
public class ListModesCommand : AsyncCommand<ListModesCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ListModesCommandSettings settings)
    {
        try
        {
            ShowAvailableModes(settings.Verbose);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Error:[/] {ex.Message}");
            return 1;
        }
    }
    
    private void ShowAvailableModes(bool verbose)
    {
        AnsiConsole.MarkupLine("[bold blue]Available Parsing Modes:[/]");
        
        var table = new Table()
            .AddColumn("[bold blue]Mode[/]")
            .AddColumn("[bold blue]Status[/]")
            .AddColumn("[bold blue]Description[/]")
            .AddColumn("[bold blue]Requirements[/]");
        
        // Remote mode (always available)
        table.AddRow(
            "[blue]Remote[/]",
            "[green]Available[/]",
            "Parse using PlantUML online service",
            "Internet connection"
        );
        
        // Local mode (may not be available)
        var localAvailable = CheckLocalModeAvailability();
        table.AddRow(
            "[green]Local[/]",
            localAvailable ? "[green]Available[/]" : "[red]Not Available[/]",
            "Parse using local PlantUML.jar",
            localAvailable ? "Java + PlantUML.jar" : "Java + PlantUML.jar (not found)"
        );
        
        // Embedded mode (always available)
        table.AddRow(
            "[yellow]Embedded[/]",
            "[green]Available[/]",
            "Parse using embedded PlantUML",
            "Built-in parser"
        );
        
        AnsiConsole.Write(table);
        
        if (!localAvailable && verbose)
        {
            AnsiConsole.WriteLine();
            var panel = new Panel(
                "[bold yellow]To enable Local mode:[/]\n\n" +
                "1. Install Java (JRE 8+)\n" +
                "2. Download plantuml.jar from https://plantuml.com/download\n" +
                "3. Place plantuml.jar in your working directory\n" +
                "4. Or set JAVA_HOME environment variable"
            )
            {
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 1)
            };
            AnsiConsole.Write(panel);
        }
        
        if (verbose)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold yellow]Usage Examples:[/]");
            AnsiConsole.MarkupLine("  pumlplaner parse file.puml --mode Local");
            AnsiConsole.MarkupLine("  pumlplaner parse file.puml --mode Remote");
            AnsiConsole.MarkupLine("  pumlplaner parse file.puml --mode Embedded");
        }
    }
    
    private bool CheckLocalModeAvailability()
    {
        try
        {
            // Check if Java is available
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(javaHome))
            {
                var javaExe = Path.Combine(javaHome, "bin", "java.exe");
                if (File.Exists(javaExe))
                    return true;
            }
            
            // Check if plantuml.jar exists
            var plantUmlJar = "plantuml.jar";
            if (File.Exists(plantUmlJar))
                return true;
            
            return false;
        }
        catch
        {
            return false;
        }
    }
}
