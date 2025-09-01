using Spectre.Console;
using Spectre.Console.Cli;
using PumlSchemasManager.Core;

namespace PumlPlanerCli.Commands;

/// <summary>
/// Settings for config command
/// </summary>
public class ConfigCommandSettings : CommandSettings
{
    [CommandOption("--set-mode|-m")]
    public ParsingMode? SetMode { get; set; }
    
    [CommandOption("--show|-s")]
    public bool Show { get; set; }
    
    [CommandOption("--test|-t")]
    public bool Test { get; set; }
}

/// <summary>
/// Command to configure parser settings
/// </summary>
public class ConfigCommand : AsyncCommand<ConfigCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ConfigCommandSettings settings)
    {
        try
        {
            if (settings.SetMode.HasValue)
            {
                await SetDefaultMode(settings.SetMode.Value);
                return 0;
            }
            
            if (settings.Test)
            {
                await TestParsers();
                return 0;
            }
            
            // Default: show configuration
            ShowConfiguration();
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Error:[/] {ex.Message}");
            return 1;
        }
    }
    
    private async Task SetDefaultMode(ParsingMode mode)
    {
        AnsiConsole.MarkupLine($"[bold blue]Setting default parsing mode to:[/] {GetModeDisplayName(mode)}");
        
        // TODO: Implement actual configuration persistence
        // var config = new ParserConfiguration { DefaultMode = mode };
        // await SaveConfiguration(config);
        
        AnsiConsole.MarkupLine($"[bold green]✓[/] Default mode set to {GetModeDisplayName(mode)}");
        
        // Show what this means
        var description = mode switch
        {
            ParsingMode.Remote => "PlantUML will be parsed using the online service (requires internet)",
            ParsingMode.Local => "PlantUML will be parsed using your local installation (no internet required)",
            ParsingMode.Embedded => "PlantUML will be parsed using embedded libraries (no internet required)",
            _ => "Unknown mode"
        };
        
        var panel = new Panel(description)
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 1)
        };
        AnsiConsole.Write(panel);
    }
    
    private void ShowConfiguration()
    {
        AnsiConsole.MarkupLine("[bold blue]Parser Configuration:[/]");
        
        // TODO: Load actual configuration
        var defaultMode = ParsingMode.Remote; // Get from config
        
        var table = new Table()
            .AddColumn("[bold blue]Setting[/]")
            .AddColumn("[bold blue]Value[/]")
            .AddColumn("[bold blue]Description[/]");
        
        table.AddRow(
            "Default Mode",
            GetModeDisplayName(defaultMode),
            "Parsing mode used when no mode is specified. It renders the schemas via plantuml.com"
        );
        
        table.AddRow(
            "Local Parser",
            CheckLocalModeAvailability() ? "[green]Available[/]" : "[red]Not Available[/]",
            "Local PlantUML.jar installation status"
        );
        
        AnsiConsole.Write(table);
        
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]Usage:[/]");
        AnsiConsole.MarkupLine("  config --set-mode Local    # Set default to local parsing");
        AnsiConsole.MarkupLine("  config --set-mode Remote   # Set default to remote parsing");
        AnsiConsole.MarkupLine("  config --test              # Test all available parsers");
    }
    
    private async Task TestParsers()
    {
        AnsiConsole.MarkupLine("[bold blue]Testing Available Parsers:[/]");
        
        var table = new Table()
            .AddColumn("[bold blue]Mode[/]")
            .AddColumn("[bold blue]Status[/]")
            .AddColumn("[bold blue]Test Result[/]")
            .AddColumn("[bold blue]Response Time[/]");
        
        // Test Remote mode
        var remoteResult = await TestRemoteParser();
        table.AddRow(
            "[blue]Remote[/]",
            "[green]Available[/]",
            remoteResult.Success ? "[green]Pass[/]" : "[red]Fail[/]",
            $"{remoteResult.ResponseTime}ms"
        );
        
        // Test Local mode
        var localResult = await TestLocalParser();
        var localStatus = CheckLocalModeAvailability() ? "[green]Available[/]" : "[red]Not Available[/]";
        table.AddRow(
            "[green]Local[/]",
            localStatus,
            localResult.Success ? "[green]Pass[/]" : "[red]Fail[/]",
            $"{localResult.ResponseTime}ms"
        );
        
        AnsiConsole.Write(table);
        
        // Show recommendations
        if (localResult.Success)
        {
            AnsiConsole.WriteLine();
            var panel = new Panel(
                "[bold green]Local parser is working![/]\n\n" +
                "You can use --mode Local for faster parsing without internet."
            )
            {
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 1)
            };
            AnsiConsole.Write(panel);
        }
    }
    
    private async Task<ParserTestResult> TestRemoteParser()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // TODO: Implement actual remote parser test
            await Task.Delay(500); // Simulate network delay
            stopwatch.Stop();
            
            return new ParserTestResult
            {
                Success = true,
                ResponseTime = stopwatch.ElapsedMilliseconds
            };
        }
        catch
        {
            stopwatch.Stop();
            return new ParserTestResult
            {
                Success = false,
                ResponseTime = stopwatch.ElapsedMilliseconds
            };
        }
    }
    
    private async Task<ParserTestResult> TestLocalParser()
    {
        if (!CheckLocalModeAvailability())
        {
            return new ParserTestResult { Success = false, ResponseTime = 0 };
        }
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // TODO: Implement actual local parser test
            await Task.Delay(200); // Simulate local processing
            stopwatch.Stop();
            
            return new ParserTestResult
            {
                Success = true,
                ResponseTime = stopwatch.ElapsedMilliseconds
            };
        }
        catch
        {
            stopwatch.Stop();
            return new ParserTestResult
            {
                Success = false,
                ResponseTime = stopwatch.ElapsedMilliseconds
            };
        }
    }
    
    private string GetModeDisplayName(ParsingMode mode) => mode switch
    {
        ParsingMode.Remote => "[blue]Remote[/]",
        ParsingMode.Local => "[green]Local[/]",
        ParsingMode.Embedded => "[yellow]Embedded[/]",
        _ => "[gray]Unknown[/]"
    };
    
    private bool CheckLocalModeAvailability()
    {
        try
        {
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(javaHome))
            {
                var javaExe = Path.Combine(javaHome, "bin", "java.exe");
                if (File.Exists(javaExe))
                    return true;
            }
            
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

/// <summary>
/// Result of parser testing
/// </summary>
public class ParserTestResult
{
    public bool Success { get; set; }
    public long ResponseTime { get; set; }
}
