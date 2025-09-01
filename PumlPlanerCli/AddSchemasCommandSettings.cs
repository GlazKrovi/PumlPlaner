using Spectre.Console.Cli;

namespace PumlPlanerCli;

/// <summary>
///     Settings for add schemas command
/// </summary>
public class AddSchemasCommandSettings : CommandSettings
{
    [CommandArgument(0, "<projectId>")] public string ProjectId { get; set; } = string.Empty;

    [CommandArgument(1, "<schemas...>")] public string[] SchemaFiles { get; set; } = [];
}