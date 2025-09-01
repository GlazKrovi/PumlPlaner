using Spectre.Console;
using Spectre.Console.Cli;
using Microsoft.Extensions.DependencyInjection;
using PumlSchemasManager.Commands;
using PumlSchemasManager.Application;
using PumlSchemasManager.Core;
using PumlSchemasManager.Infrastructure;
using PumlSchemasManager.Domain;
using LiteDB;

namespace PumlPlanerCli;

/// <summary>
/// Main CLI application for PumlPlaner
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Configure dependency injection
        var services = ConfigureServices();
        
        // Create command app
        var app = new CommandApp(new TypeRegistrar(services));
        
        // Configure CLI
        app.Configure(config =>
        {
            config.SetApplicationName("pumlplaner");
            config.SetApplicationVersion("1.0.0");
            
            config.AddCommand<ParseCommand>("parse", "Parse a PlantUML file")
                .WithDescription("Parse and analyze a single PlantUML file")
                .WithExample(new[] { "parse", "path/to/file.puml" });
                
            config.AddCommand<DiscoverCommand>("discover", "Discover PlantUML files in a folder")
                .WithDescription("Find all PlantUML files in a specified directory")
                .WithExample(new[] { "discover", "path/to/folder" });
                
            config.AddCommand<CreateProjectCommand>("create-project", "Create a new project")
                .WithDescription("Create a new PumlPlaner project")
                .WithExample(new[] { "create-project", "MyProject" });
                
            config.AddCommand<AddSchemasCommand>("add-schemas", "Add schemas to a project")
                .WithDescription("Add existing schemas to a project")
                .WithExample(new[] { "add-schemas", "projectId", "schema1.puml", "schema2.puml" });
                
            config.AddCommand<DiscoverAndAddCommand>("discover-add", "Discover and add schemas to a project")
                .WithDescription("Discover PlantUML files and add them to a project")
                .WithExample(new[] { "discover-add", "projectId", "path/to/folder" });
                
            config.AddCommand<MergeCommand>("merge", "Merge multiple schemas")
                .WithDescription("Merge multiple PlantUML schemas into one")
                .WithExample(new[] { "merge", "schema1.puml", "schema2.puml" });
                
            config.AddCommand<GenerateCommand>("generate", "Generate output files")
                .WithDescription("Generate output files from a project")
                .WithExample(new[] { "generate", "projectId", "png", "svg" });
        });
        
        // Show welcome message if no arguments
        if (args.Length == 0)
        {
            ShowWelcomeMessage();
            return await app.RunAsync(new[] { "--help" });
        }
        
        return await app.RunAsync(args);
    }
    
    /// <summary>
    /// Configure dependency injection services
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Register core services
        services.AddSingleton<IParser, PlantUmlParser>();
        services.AddSingleton<IFileDiscoveryService, FileSystemDiscoveryService>();
        services.AddSingleton<SchemaManager>();
        
        // Register commands
        services.AddTransient<ParseCommand>();
        services.AddTransient<DiscoverCommand>();
        services.AddTransient<CreateProjectCommand>();
        services.AddTransient<AddSchemasCommand>();
        services.AddTransient<DiscoverAndAddCommand>();
        services.AddTransient<MergeCommand>();
        services.AddTransient<GenerateCommand>();
        
        return services.BuildServiceProvider();
    }
    
    /// <summary>
    /// Display a beautiful welcome message
    /// </summary>
    private static void ShowWelcomeMessage()
    {
        AnsiConsole.Clear();
        
        // Create a beautiful header
        var rule = new Rule("[bold blue]PumlPlaner CLI[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("blue dim")
        };
        
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
        
        // Display project info
        var panel = new Panel(
            "[bold green]Welcome to PumlPlaner![/]\n\n" +
            "A powerful CLI tool for managing and analyzing PlantUML schemas.\n" +
            "Use [bold blue]--help[/] to see all available commands."
        )
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 1)
        };
        
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
        
        // Show quick start examples
        var table = new Table()
            .AddColumn("[bold blue]Command[/]")
            .AddColumn("[bold blue]Description[/]")
            .AddColumn("[bold blue]Example[/]");
            
        table.AddRow("parse", "Parse a PlantUML file", "parse file.puml");
        table.AddRow("discover", "Find PlantUML files", "discover ./src");
        table.AddRow("create-project", "Create new project", "create-project MyApp");
        table.AddRow("generate", "Generate outputs", "generate projectId png svg");
        
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }
}

/// <summary>
/// Command to parse a PlantUML file
/// </summary>
public class ParseCommand : AsyncCommand<ParseCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<file>")]
        public string FilePath { get; set; } = string.Empty;
        
        [CommandOption("--verbose|-v")]
        public bool Verbose { get; set; }
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            AnsiConsole.Status()
                .Start("Parsing PlantUML file...", ctx => 
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.Status($"Reading {settings.FilePath}");
                });
            
            // TODO: Implement actual parsing logic
            await Task.Delay(1000); // Simulate work
            
            var result = new { IsSuccess = true, Message = "File parsed successfully" };
            
            if (result.IsSuccess)
            {
                AnsiConsole.MarkupLine("[bold green]✓[/] File parsed successfully!");
                
                if (settings.Verbose)
                {
                    var panel = new Panel($"[bold blue]File:[/] {settings.FilePath}")
                    {
                        Border = BoxBorder.Rounded,
                        Padding = new Padding(1, 1)
                    };
                    AnsiConsole.Write(panel);
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[bold red]✗[/] Failed to parse file");
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Error:[/] {ex.Message}");
            return 1;
        }
    }
}

/// <summary>
/// Command to discover PlantUML files in a folder
/// </summary>
public class DiscoverCommand : AsyncCommand<DiscoverCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<folder>")]
        public string FolderPath { get; set; } = string.Empty;
        
        [CommandOption("--recursive|-r")]
        public bool Recursive { get; set; }
        
        [CommandOption("--pattern|-p")]
        public string Pattern { get; set; } = "*.puml";
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            AnsiConsole.MarkupLine($"[bold blue]Discovering PlantUML files in:[/] {settings.FolderPath}");
            
            // TODO: Implement actual discovery logic
            await Task.Delay(500);
            
            var files = new[] { "schema1.puml", "schema2.puml", "diagram.puml" };
            
            var table = new Table()
                .AddColumn("[bold blue]File[/]")
                .AddColumn("[bold blue]Size[/]")
                .AddColumn("[bold blue]Last Modified[/]");
                
            foreach (var file in files)
            {
                table.AddRow(file, "2.3 KB", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
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

/// <summary>
/// Command to create a new project
/// </summary>
public class CreateProjectCommand : AsyncCommand<CreateProjectCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<name>")]
        public string ProjectName { get; set; } = string.Empty;
        
        [CommandOption("--description|-d")]
        public string Description { get; set; } = string.Empty;
        
        [CommandOption("--output|-o")]
        public string OutputPath { get; set; } = "./";
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            AnsiConsole.MarkupLine($"[bold blue]Creating project:[/] {settings.ProjectName}");
            
            // TODO: Implement actual project creation
            await Task.Delay(800);
            
            var projectId = Guid.NewGuid().ToString("N")[..8];
            
            AnsiConsole.MarkupLine($"[bold green]✓[/] Project created successfully!");
            
            var panel = new Panel(
                $"[bold blue]Project ID:[/] {projectId}\n" +
                $"[bold blue]Name:[/] {settings.ProjectName}\n" +
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

/// <summary>
/// Command to add schemas to a project
/// </summary>
public class AddSchemasCommand : AsyncCommand<AddSchemasCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<projectId>")]
        public string ProjectId { get; set; } = string.Empty;
        
        [CommandArgument(1, "<schemas...>")]
        public string[] SchemaFiles { get; set; } = Array.Empty<string>();
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            AnsiConsole.MarkupLine($"[bold blue]Adding schemas to project:[/] {settings.ProjectId}");
            
            // TODO: Implement actual schema addition
            await Task.Delay(600);
            
            AnsiConsole.MarkupLine($"[bold green]✓[/] Added {settings.SchemaFiles.Length} schemas to project!");
            
            var table = new Table()
                .AddColumn("[bold blue]Schema File[/]")
                .AddColumn("[bold blue]Status[/]");
                
            foreach (var file in settings.SchemaFiles)
            {
                table.AddRow(file, "[green]Added[/]");
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

/// <summary>
/// Command to discover and add schemas to a project
/// </summary>
public class DiscoverAndAddCommand : AsyncCommand<DiscoverAndAddCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<projectId>")]
        public string ProjectId { get; set; } = string.Empty;
        
        [CommandArgument(1, "<folder>")]
        public string FolderPath { get; set; } = string.Empty;
        
        [CommandOption("--recursive|-r")]
        public bool Recursive { get; set; }
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            AnsiConsole.MarkupLine($"[bold blue]Discovering and adding schemas to project:[/] {settings.ProjectId}");
            
            // TODO: Implement actual discovery and addition
            await Task.Delay(1000);
            
            var discoveredFiles = new[] { "schema1.puml", "schema2.puml" };
            
            AnsiConsole.MarkupLine($"[bold green]✓[/] Discovered and added {discoveredFiles.Length} schemas!");
            
            var table = new Table()
                .AddColumn("[bold blue]Discovered File[/]")
                .AddColumn("[bold blue]Action[/]");
                
            foreach (var file in discoveredFiles)
            {
                table.AddRow(file, "[green]Added to project[/]");
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

/// <summary>
/// Command to merge multiple schemas
/// </summary>
public class MergeCommand : AsyncCommand<MergeCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<schemas...>")]
        public string[] SchemaFiles { get; set; } = Array.Empty<string>();
        
        [CommandOption("--output|-o")]
        public string OutputFile { get; set; } = "merged.puml";
        
        [CommandOption("--format|-f")]
        public string Format { get; set; } = "puml";
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            AnsiConsole.MarkupLine($"[bold blue]Merging {settings.SchemaFiles.Length} schemas...[/]");
            
            // TODO: Implement actual merging logic
            await Task.Delay(1200);
            
            AnsiConsole.MarkupLine($"[bold green]✓[/] Schemas merged successfully!");
            
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

/// <summary>
/// Command to generate output files
/// </summary>
public class GenerateCommand : AsyncCommand<GenerateCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<projectId>")]
        public string ProjectId { get; set; } = string.Empty;
        
        [CommandArgument(1, "<formats...>")]
        public string[] Formats { get; set; } = Array.Empty<string>();
        
        [CommandOption("--output|-o")]
        public string OutputPath { get; set; } = "./output";
        
        [CommandOption("--quality|-q")]
        public int Quality { get; set; } = 100;
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            AnsiConsole.MarkupLine($"[bold blue]Generating outputs for project:[/] {settings.ProjectId}");
            
            // TODO: Implement actual generation logic
            await Task.Delay(1500);
            
            AnsiConsole.MarkupLine($"[bold green]✓[/] Output files generated successfully!");
            
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

/// <summary>
/// Type registrar for dependency injection
/// </summary>
public class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceProvider _serviceProvider;
    
    public TypeRegistrar(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public ITypeResolver Build()
    {
        return new TypeResolver(_serviceProvider);
    }
    
    public void Register(Type service, Type implementation)
    {
        // Not needed for this implementation
    }
    
    public void RegisterInstance(Type service, object implementation)
    {
        // Not needed for this implementation
    }
    
    public void RegisterLazy(Type service, Func<object> factory)
    {
        // Not needed for this implementation
    }
}

/// <summary>
/// Type resolver for dependency injection
/// </summary>
public class TypeResolver : ITypeResolver
{
    private readonly IServiceProvider _serviceProvider;
    
    public TypeResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public object Resolve(Type type)
    {
        return _serviceProvider.GetService(type) ?? 
               throw new InvalidOperationException($"Service of type {type} not registered");
    }
}
