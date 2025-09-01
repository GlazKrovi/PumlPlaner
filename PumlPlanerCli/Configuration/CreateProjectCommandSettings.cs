using Spectre.Console.Cli;

namespace PumlPlanerCli;

/// <summary>
///     Settings for create project command
/// </summary>
public class CreateProjectCommandSettings : CommandSettings
{
    [CommandArgument(0, "<name>")] public string ProjectName { get; set; } = string.Empty;

    [CommandOption("--description|-d")] public string Description { get; set; } = string.Empty;

    [CommandOption("--output|-o")] public string OutputPath { get; set; } = "./";
}