using PlantUml.Net;

namespace PumlSchemasManager.Utils;

public class Parser
{
    private static readonly string GeneratedPath;
    
    static Parser()
    {
        string exeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();
        GeneratedPath = Path.Combine(exeDirectory, "generated");
        
        if (!Directory.Exists(GeneratedPath))
        {
            Directory.CreateDirectory(GeneratedPath);
        }
    }
    
    public string Parse(string source)
    {
        var generator = new PlantUmlRenderer();
        var pngBytes = generator.Render(source, OutputFormat.Png);

        
        string fileName = $"{Guid.NewGuid()}.png";
        string filePath = Path.Combine(GeneratedPath, fileName);
        
        
        File.WriteAllBytes(filePath, pngBytes);
        
        
        return filePath;
    }
    
    public string GetGeneratedPath()
    {
        return GeneratedPath;
    }
}
