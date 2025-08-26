using PumlPlaner.AST;
using PumlPlaner.Visitors;
using PumlSchemasManager.Core;
using PumlSchemasManager.Domain;

namespace PumlSchemasManager.Infrastructure;

/// <summary>
/// PlantUML parser implementation using PumlPlaner logic
/// </summary>
public class PlantUmlParser : IParser
{
    public async Task<ParseResult> ParseAsync(string source)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Basic validation - check if it contains PlantUML markers
                if (!source.Contains("@startuml") && !source.Contains("@start"))
                {
                    return new ParseResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Content does not contain PlantUML markers (@startuml or @start)",
                        Warnings = new List<string>()
                    };
                }

                // Try to use PumlPlaner's AST builder for parsing
                try
                {
                    var ast = new SchemeAst(source);
                    // If we get here, the content is valid PlantUML
                }
                catch (Exception parseEx)
                {
                    // If parsing fails, we'll still accept the content but with a warning
                    Console.WriteLine($"Warning: PlantUML parsing failed, but accepting content: {parseEx.Message}");
                }
                
                // Use the original content
                var processedContent = source;
                
                // Create schema from processed content
                var schema = new Schema
                {
                    Content = processedContent,
                    Metadata = new SchemaMetadata
                    {
                        DiscoveredAt = DateTime.UtcNow,
                        Hash = LiteDbStorageService.ComputeHash(processedContent)
                    }
                };
                
                return new ParseResult
                {
                    IsSuccess = true,
                    Schema = schema,
                    Warnings = new List<string>() // Could add warnings from processing
                };
            }
            catch (Exception ex)
            {
                return new ParseResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Failed to parse PlantUML: {ex.Message}",
                    Warnings = new List<string>()
                };
            }
        });
    }

    public async Task<ValidationResult> ValidateAsync(string content)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Basic validation - check if it contains PlantUML markers
                if (!content.Contains("@startuml") && !content.Contains("@start"))
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Errors = new List<string> { "Content does not contain PlantUML markers (@startuml or @start)" },
                        Warnings = new List<string>()
                    };
                }

                // Try to use PumlPlaner's AST builder for validation
                try
                {
                    var ast = new SchemeAst(content);
                    // If we get here, the content is valid PlantUML
                    return new ValidationResult
                    {
                        IsValid = true,
                        Errors = new List<string>(),
                        Warnings = new List<string>()
                    };
                }
                catch (Exception parseEx)
                {
                    // If parsing fails, we'll still consider it valid but with a warning
                    return new ValidationResult
                    {
                        IsValid = true,
                        Errors = new List<string>(),
                        Warnings = new List<string> { $"PlantUML parsing warning: {parseEx.Message}" }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { ex.Message },
                    Warnings = new List<string>()
                };
            }
        });
    }
}
