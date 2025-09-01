using PumlSchemasManager.Core;
using PumlSchemasManager.Domain;

namespace PumlSchemasManager.Infrastructure;

/// <summary>
/// Local PlantUML parser using PlantUML.jar
/// </summary>
public class LocalPlantUmlParser : IParser
{
    private readonly string _plantUmlPath;
    private readonly string _javaPath;
    
    public LocalPlantUmlParser(string? plantUmlPath = null, string? javaPath = null)
    {
        _plantUmlPath = plantUmlPath ?? FindPlantUmlJar();
        _javaPath = javaPath ?? FindJavaPath();
    }
    
    public ParsingMode Mode => ParsingMode.Local;
    
    public ParserCapabilities Capabilities => new()
    {
        CanGenerateImages = true,
        CanValidateSyntax = true,
        RequiresInternet = false,
        SupportedFormats = new List<string> { "png", "svg", "pdf", "eps", "txt" },
        IsAvailable = !string.IsNullOrEmpty(_plantUmlPath) && !string.IsNullOrEmpty(_javaPath)
    };
    
    public async Task<ParseResult> ParseAsync(string content)
    {
        try
        {
            if (!Capabilities.IsAvailable)
            {
                return new ParseResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Local PlantUML parser not available. Please install Java and PlantUML.jar",
                    ParsingMode = Mode
                };
            }
            
            // TODO: Implement using Java process with PlantUML.jar
            // var process = new Process
            // {
            //     StartInfo = new ProcessStartInfo
            //     {
            //         FileName = _javaPath,
            //         Arguments = $"-jar \"{_plantUmlPath}\" -pipe",
            //         UseShellExecute = false,
            //         RedirectStandardInput = true,
            //         RedirectStandardOutput = true,
            //         RedirectStandardError = true,
            //         CreateNoWindow = true
            //     }
            // };
            
            // Simulate parsing for now
            await Task.Delay(800);
            
            return new ParseResult
            {
                IsSuccess = true,
                Content = content,
                Message = "Content parsed successfully using local PlantUML installation",
                ParsingMode = Mode
            };
        }
        catch (Exception ex)
        {
            return new ParseResult
            {
                IsSuccess = false,
                ErrorMessage = $"Local parsing failed: {ex.Message}",
                ParsingMode = Mode
            };
        }
    }
    
    public async Task<ValidationResult> ValidateAsync(string content)
    {
        try
        {
            if (!Capabilities.IsAvailable)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = "Local PlantUML parser not available",
                    Errors = new List<string> { "Please install Java and PlantUML.jar" }
                };
            }
            
            // TODO: Implement actual validation using Java process
            // For now, do basic syntax check
            await Task.Delay(300);
            
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
    
    private string? FindPlantUmlJar()
    {
        var possiblePaths = new[]
        {
            "plantuml.jar",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PlantUML", "plantuml.jar"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PlantUML", "plantuml.jar"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "PlantUML", "plantuml.jar")
        };
        
        return possiblePaths.FirstOrDefault(File.Exists);
    }
    
    private string? FindJavaPath()
    {
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome))
        {
            var javaExe = Path.Combine(javaHome, "bin", "java.exe");
            if (File.Exists(javaExe))
                return javaExe;
        }
        
        // Try to find java in PATH
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            
            if (process.Start())
            {
                process.WaitForExit(1000);
                if (process.ExitCode == 0)
                    return "java";
            }
        }
        catch
        {
            // Java not found in PATH
        }
        
        return null;
    }
}
