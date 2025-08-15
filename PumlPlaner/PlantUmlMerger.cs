using System.Text;
using PumlPlaner.Helpers;
using PumlPlaner.Visitors;

namespace PumlPlaner;

public class PlantUmlMerger : PumlgBaseVisitor<string>
{
    public string VisitUml(params PumlgParser.UmlContext[] contexts)
    {
        var sb = new StringBuilder();
        sb.AppendLine("@startuml");

        foreach (var ctx in contexts)
        {
            foreach (var child in ctx.children)
            {
                var text = child.GetText();
                if (text is "@startuml" or "@enduml")
                    continue;

                var merged = Visit(child);
                if (!string.IsNullOrWhiteSpace(merged))
                    sb.AppendLine(merged.TrimEnd());
            }
        }

        sb.AppendLine("@enduml");
        return StringHelper.NormalizeBreakLines(sb.ToString());
    }
}