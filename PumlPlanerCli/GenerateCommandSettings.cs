using Spectre.Console.Cli;

namespace PumlPlanerCli;

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