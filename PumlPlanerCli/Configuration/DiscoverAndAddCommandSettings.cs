using Spectre.Console.Cli;

namespace PumlPlanerCli;

/// <summary>
///     Settings for discover and add command
/// </summary>
public class DiscoverAndAddCommandSettings : CommandSettings
{
    [CommandArgument(0, "<projectId>")] public string ProjectId { get; set; } = string.Empty;

    [CommandArgument(1, "<folder>")] public string FolderPath { get; set; } = string.Empty;

    [CommandOption("--recursive|-r")] public bool Recursive { get; set; }
}