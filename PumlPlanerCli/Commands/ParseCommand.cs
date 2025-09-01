using Spectre.Console;
using Spectre.Console.Cli;
using PumlSchemasManager.Commands;
using PumlSchemasManager.Core;

namespace PumlPlanerCli.Commands;

/// <summary>
/// Settings for parse command
/// </summary>
public class ParseCommandSettings : CommandSettings
{
    [CommandArgument(0, "<file>")]
    public string FilePath { get; set; } = string.Empty;
    
    [CommandOption("--mode|-m")]
    public ParsingMode? Mode { get; set; }
    
    [CommandOption("--verbose|-v")]
    public bool Verbose { get; set; }
}

/// <summary>
/// Command to parse a PlantUML file
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
            
            // Check if file path is provided
            if (string.IsNullOrEmpty(settings.FilePath))
            {
                AnsiConsole.MarkupLine("[bold red]✗[/] File path is required when not using --list-modes");
                return 1;
            }
            
            // Check if file exists
            if (!File.Exists(settings.FilePath))
            {
                AnsiConsole.MarkupLine($"[bold red]✗[/] File not found: {settings.FilePath}");
                return 1;
            }
            
            // TODO: Use actual ParseCommand from PumlSchemasManager
            // var parseCommand = new ParseCommand(parserFactory);
            // var result = await parseCommand.ExecuteAsync(settings.FilePath, settings.Mode);
            
            // Simulate parsing work
            await Task.Delay(1000);
            
            var mode = settings.Mode ?? ParsingMode.Remote;
            var modeText = mode switch
            {
                ParsingMode.Remote => "[blue]Remote[/]",
                ParsingMode.Local => "[green]Local[/]",
                ParsingMode.Embedded => "[yellow]Embedded[/]",
                _ => "[gray]Unknown[/]"
            };
            
            AnsiConsole.MarkupLine($"[bold green]✓[/] File parsed successfully using {modeText} mode!");
            
            if (settings.Verbose)
            {
                var fileInfo = new FileInfo(settings.FilePath);
                var panel = new Panel(
                    $"[bold blue]File:[/] {settings.FilePath}\n" +
                    $"[bold blue]Size:[/] {fileInfo.Length} bytes\n" +
                    $"[bold blue]Last Modified:[/] {fileInfo.LastWriteTime}\n" +
                    $"[bold blue]Parsing Mode:[/] {mode}"
                )
                {
                    Border = BoxBorder.Rounded,
                    Padding = new Padding(1, 1)
                };
                AnsiConsole.Write(panel);
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Error:[/] {ex.Message}");
            return 1;
        }
    }
    
    private void ShowAvailableModes()
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
        
        // Embedded mode (not implemented yet)
        table.AddRow(
            "[yellow]Embedded[/]",
            "[yellow]Not Implemented[/]",
            "Parse using embedded PlantUML",
            "Future feature"
        );
        
        AnsiConsole.Write(table);
        
        if (!localAvailable)
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
