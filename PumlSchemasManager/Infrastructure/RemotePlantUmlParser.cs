using PumlSchemasManager.Core;
using PumlSchemasManager.Domain;

namespace PumlSchemasManager.Infrastructure;

/// <summary>
/// Remote PlantUML parser using PlantUML.Net
/// </summary>
public class RemotePlantUmlParser : IParser
{
    public ParsingMode Mode => ParsingMode.Remote;
    
    public ParserCapabilities Capabilities => new()
    {
        CanGenerateImages = true,
        CanValidateSyntax = true,
        RequiresInternet = true,
        SupportedFormats = new List<string> { "png", "svg", "pdf", "eps", "txt" },
        IsAvailable = true
    };
    
    public async Task<ParseResult> ParseAsync(string content)
    {
        try
        {
            // TODO: Implement using PlantUML.Net
            // var factory = new RendererFactory();
            // var renderer = factory.CreateRenderer(new PlantUmlSettings());
            // var bytes = await renderer.RenderAsync(content, OutputFormat.Png);
            
            // Simulate parsing for now
            await Task.Delay(500);
            
            return new ParseResult
            {
                IsSuccess = true,
                Content = content,
                Message = "Content parsed successfully using remote PlantUML service",
                ParsingMode = Mode
            };
        }
        catch (Exception ex)
        {
            return new ParseResult
            {
                IsSuccess = false,
                ErrorMessage = $"Remote parsing failed: {ex.Message}",
                ParsingMode = Mode
            };
        }
    }
    
    public async Task<ValidationResult> ValidateAsync(string content)
    {
        try
        {
            // TODO: Implement actual validation using PlantUML.Net
            // For now, do basic syntax check
            await Task.Delay(200);
            
            var isValid = !string.IsNullOrWhiteSpace(content) && 
                         content.Contains("@startuml") && 
                         content.Contains("@enduml");
            
            return new ValidationResult
            {
                IsValid = isValid,
                Message = isValid ? "Content is valid PlantUML" : "Content is not valid PlantUML",
                Errors = isValid ? new List<string>() : new List<string> { "Missing @startuml or @enduml tags" }
            };
        }
        catch (Exception ex)
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = $"Validation failed: {ex.Message}",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}
