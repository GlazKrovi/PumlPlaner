using PlantUml.Net;
using System.IO;
using PumlSchemasManager.Utils;

namespace PumlSchemasManager;

public class Parse 
{
    public void From(string sourceFile)
    {
        var parser = new Parser();
        var pngPath = parser.Parse(sourceFile);
        Console.WriteLine(pngPath);
    }
}
