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