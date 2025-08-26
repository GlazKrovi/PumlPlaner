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
                // Use PumlPlaner's AST builder for parsing
                var ast = new SchemeAst(source);
                
                // For now, use the original content
                // In a real implementation, apply PumlPlaner's visitors for processing
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
                // Basic validation using PumlPlaner
                var ast = new SchemeAst(content);
                
                // If we get here, the content is valid PlantUML
                return new ValidationResult
                {
                    IsValid = true,
                    Errors = new List<string>(),
                    Warnings = new List<string>()
                };
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
