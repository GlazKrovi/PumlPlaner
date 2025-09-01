using Spectre.Console.Cli;

namespace PumlPlanerCli;

/// <summary>
///     Settings for discover command
/// </summary>
public class DiscoverCommandSettings : CommandSettings
{
    [CommandArgument(0, "<folder>")] public string FolderPath { get; set; } = string.Empty;

    [CommandOption("--recursive|-r")] public bool Recursive { get; set; }

    [CommandOption("--pattern|-p")] public string Pattern { get; set; } = "*.puml";
}