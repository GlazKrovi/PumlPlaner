using Spectre.Console.Cli;

namespace PumlPlanerCli;

/// <summary>
///     Settings for parse command
/// </summary>
public class ParseCommandSettings : CommandSettings
{
    [CommandArgument(0, "<file>")] public string FilePath { get; set; } = string.Empty;

    [CommandOption("--verbose|-v")] public bool Verbose { get; set; }
}