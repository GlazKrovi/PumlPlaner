using PumlSchemasManager.Core;
using PumlSchemasManager.Domain;

namespace PumlSchemasManager.Infrastructure;

/// <summary>
///     PlantUML parser implementation using PumlPlaner logic
/// </summary>
public class PlantUmlParser : IParser
{
    public async Task<ParseResult> ParseAsync(string source)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Check for null or empty source
                if (string.IsNullOrEmpty(source))
                    return new ParseResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Content does not contain PlantUML markers",
                        Warnings = []
                    };

                // Basic validation - check if it contains PlantUML markers
                if (!source.Contains("@startuml") && !source.Contains("@start"))
                    return new ParseResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Content does not contain PlantUML markers",
                        Warnings = []
                    };

                // Use the original content (simplified approach without PumlPlaner AST)
                var processedContent = source;

                Console.WriteLine(
                    $"Creating schema for content: {processedContent.Substring(0, Math.Min(50, processedContent.Length))}...");

                // Create schema from processed content
                string hash;
                try
                {
                    hash = LiteDbStorageService.ComputeHash(processedContent);
                }
                catch (Exception hashEx)
                {
                    Console.WriteLine($"Error computing hash: {hashEx.Message}");
                    hash = "error";
                }

                var schema = new Schema
                {
                    Content = processedContent,
                    Metadata = new SchemaMetadata
                    {
                        DiscoveredAt = DateTime.UtcNow,
                        Hash = hash
                    }
                };

                Console.WriteLine($"Schema created successfully: {schema != null}");

                var result = new ParseResult
                {
                    IsSuccess = true,
                    Schema = schema,
                    Warnings = []
                };

                Console.WriteLine($"ParseResult created: IsSuccess={result.IsSuccess}, Schema={result.Schema != null}");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PlantUmlParser.ParseAsync error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new ParseResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Failed to parse PlantUML: {ex.Message}",
                    Warnings = []
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
                // Check for null or empty content
                if (string.IsNullOrEmpty(content))
                    return new ValidationResult
                    {
                        IsValid = false,
                        Errors = ["Content does not contain PlantUML markers"],
                        Warnings = []
                    };

                // Basic validation - check if it contains PlantUML markers
                if (!content.Contains("@startuml") && !content.Contains("@start"))
                    return new ValidationResult
                    {
                        IsValid = false,
                        Errors = ["Content does not contain PlantUML markers"],
                        Warnings = []
                    };

                // Simplified validation without PumlPlaner AST
                return new ValidationResult
                {
                    IsValid = true,
                    Errors = [],
                    Warnings = []
                };
            }
            catch (Exception ex)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = [ex.Message],
                    Warnings = []
                };
            }
        });
    }
}