using Spectre.Console.Cli;

namespace PumlPlanerCli;

/// <summary>
///     Settings for merge command
/// </summary>
public class MergeCommandSettings : CommandSettings
{
    [CommandArgument(0, "<schemas...>")] public string[] SchemaFiles { get; set; } = [];

    [CommandOption("--output|-o")] public string OutputFile { get; set; } = "merged.puml";

    [CommandOption("--format|-f")] public string Format { get; set; } = "puml";
}