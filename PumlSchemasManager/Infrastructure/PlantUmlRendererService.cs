using System.Text;
using PumlSchemasManager.Core;
using PumlSchemasManager.Domain;

namespace PumlSchemasManager.Infrastructure;

/// <summary>
/// PlantUML renderer service (simplified version)
/// </summary>
public class PlantUmlRendererService : IRendererService
{
    public async Task<byte[]> RenderAsync(Schema schema, SchemaOutputFormat format)
    {
        if (schema == null)
        {
            return await RenderContentAsync("", format);
        }
        return await RenderContentAsync(schema.Content, format);
    }

    public async Task<byte[]> RenderContentAsync(string content, SchemaOutputFormat format)
    {
        return await Task.Run(() =>
        {
            // For now, return a simple placeholder
            // In a real implementation, this would use PlantUml.Net or call PlantUML directly
            var placeholderText = $"Generated {format} for PlantUML content";
            return Encoding.UTF8.GetBytes(placeholderText);
        });
    }

    public string GetContentType(SchemaOutputFormat format)
    {
        return format switch
        {
            SchemaOutputFormat.Png => "image/png",
            SchemaOutputFormat.Svg => "image/svg+xml",
            SchemaOutputFormat.Pdf => "application/pdf",
            SchemaOutputFormat.Eps => "application/postscript",
            SchemaOutputFormat.Vdx => "application/vnd.ms-visio.drawing",
            SchemaOutputFormat.Xmi => "application/xml",
            SchemaOutputFormat.Scxml => "application/xml",
            SchemaOutputFormat.Html => "text/html",
            SchemaOutputFormat.Txt => "text/plain",
            SchemaOutputFormat.Utxt => "text/plain",
            SchemaOutputFormat.Latex => "application/x-latex",
            SchemaOutputFormat.LatexNoPreamble => "application/x-latex",
            _ => "application/octet-stream"
        };
    }

    public string GetFileExtension(SchemaOutputFormat format)
    {
        return format switch
        {
            SchemaOutputFormat.Png => "png",
            SchemaOutputFormat.Svg => "svg",
            SchemaOutputFormat.Pdf => "pdf",
            SchemaOutputFormat.Eps => "eps",
            SchemaOutputFormat.Vdx => "vdx",
            SchemaOutputFormat.Xmi => "xmi",
            SchemaOutputFormat.Scxml => "scxml",
            SchemaOutputFormat.Html => "html",
            SchemaOutputFormat.Txt => "txt",
            SchemaOutputFormat.Utxt => "utxt",
            SchemaOutputFormat.Latex => "tex",
            SchemaOutputFormat.LatexNoPreamble => "tex",
            _ => "bin"
        };
    }
}
