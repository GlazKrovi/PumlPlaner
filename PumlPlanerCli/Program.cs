using Spectre.Console;
using Spectre.Console.Cli;

namespace PumlPlanerCli;

/// <summary>
///     Main CLI application for PumlPlaner
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Create command app
        var app = new CommandApp();

        // Configure CLI
        app.Configure(config =>
        {
            config.SetApplicationName("pumlplaner");
            config.SetApplicationVersion("1.0.0");

            config.AddCommand<ParseCommand>("parse")
                .WithDescription("Parse and analyze a single PlantUML file")
                .WithExample("parse", "path/to/file.puml");

            config.AddCommand<DiscoverCommand>("discover")
                .WithDescription("Find all PlantUML files in a specified directory")
                .WithExample("discover", "path/to/folder");

            config.AddCommand<CreateProjectCommand>("create-project")
                .WithDescription("Create a new PumlPlaner project")
                .WithExample("create-project", "MyProject");

            config.AddCommand<AddSchemasCommand>("add-schemas")
                .WithDescription("Add existing schemas to a project")
                .WithExample("add-schemas", "projectId", "schema1.puml", "schema2.puml");

            config.AddCommand<DiscoverAndAddCommand>("discover-add")
                .WithDescription("Discover PlantUML files and add them to a project")
                .WithExample("discover-add", "projectId", "path/to/folder");

            config.AddCommand<MergeCommand>("merge")
                .WithDescription("Merge multiple PlantUML schemas into one")
                .WithExample("merge", "schema1.puml", "schema2.puml");

            config.AddCommand<GenerateCommand>("generate")
                .WithDescription("Generate output files from a project")
                .WithExample("generate", "projectId", "png", "svg");
        });

        // Show welcome message if no arguments
        if (args.Length != 0) return await app.RunAsync(args);
        ShowWelcomeMessage();
        return await app.RunAsync(["--help"]);
    }

    /// <summary>
    ///     Display a beautiful welcome message
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
///     Settings for parse command
/// </summary>
public class ParseCommandSettings : CommandSettings
{
    [CommandArgument(0, "<file>")] public string FilePath { get; set; } = string.Empty;

    [CommandOption("--verbose|-v")] public bool Verbose { get; set; }
}

/// <summary>
///     Command to parse a PlantUML file
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

            // Check if file exists
            if (!File.Exists(settings.FilePath))
            {
                AnsiConsole.MarkupLine($"[bold red]✗[/] File not found: {settings.FilePath}");
                return 1;
            }

            // Simulate parsing work
            await Task.Delay(1000);

            AnsiConsole.MarkupLine("[bold green]✓[/] File parsed successfully!");

            if (!settings.Verbose) return 0;
            var fileInfo = new FileInfo(settings.FilePath);
            var panel = new Panel(
                $"[bold blue]File:[/] {settings.FilePath}\n" +
                $"[bold blue]Size:[/] {fileInfo.Length} bytes\n" +
                $"[bold blue]Last Modified:[/] {fileInfo.LastWriteTime}"
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
///     Settings for discover command
/// </summary>
public class DiscoverCommandSettings : CommandSettings
{
    [CommandArgument(0, "<folder>")] public string FolderPath { get; set; } = string.Empty;

    [CommandOption("--recursive|-r")] public bool Recursive { get; set; }

    [CommandOption("--pattern|-p")] public string Pattern { get; set; } = "*.puml";
}

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

/// <summary>
///     Settings for create project command
/// </summary>
public class CreateProjectCommandSettings : CommandSettings
{
    [CommandArgument(0, "<name>")] public string ProjectName { get; set; } = string.Empty;

    [CommandOption("--description|-d")] public string Description { get; set; } = string.Empty;

    [CommandOption("--output|-o")] public string OutputPath { get; set; } = "./";
}

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

/// <summary>
///     Settings for add schemas command
/// </summary>
public class AddSchemasCommandSettings : CommandSettings
{
    [CommandArgument(0, "<projectId>")] public string ProjectId { get; set; } = string.Empty;

    [CommandArgument(1, "<schemas...>")] public string[] SchemaFiles { get; set; } = [];
}

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

/// <summary>
///     Settings for discover and add command
/// </summary>
public class DiscoverAndAddCommandSettings : CommandSettings
{
    [CommandArgument(0, "<projectId>")] public string ProjectId { get; set; } = string.Empty;

    [CommandArgument(1, "<folder>")] public string FolderPath { get; set; } = string.Empty;

    [CommandOption("--recursive|-r")] public bool Recursive { get; set; }
}

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

/// <summary>
///     Settings for merge command
/// </summary>
public class MergeCommandSettings : CommandSettings
{
    [CommandArgument(0, "<schemas...>")] public string[] SchemaFiles { get; set; } = [];

    [CommandOption("--output|-o")] public string OutputFile { get; set; } = "merged.puml";

    [CommandOption("--format|-f")] public string Format { get; set; } = "puml";
}

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

/// <summary>
///     Settings for generate command
/// </summary>
public class GenerateCommandSettings : CommandSettings
{
    [CommandArgument(0, "<projectId>")] public string ProjectId { get; set; } = string.Empty;

    [CommandArgument(1, "<formats...>")] public string[] Formats { get; set; } = [];

    [CommandOption("--output|-o")] public string OutputPath { get; set; } = "./output";

    [CommandOption("--quality|-q")] public int Quality { get; set; } = 100;
}

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