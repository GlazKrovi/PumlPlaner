using System.Text;
using PumlPlaner.Helpers;

namespace PumlPlaner.Visitors;

public class PumlDeduplicator : PumlReconstructor
{
    private readonly Dictionary<string, ClassInfo> _classMap = new();

    public override string VisitUml(PumlgParser.UmlContext context)
    {
        _classMap.Clear();


        if (context.class_diagram() != null) VisitClass_diagram(context.class_diagram());


        var sb = new StringBuilder();
        sb.AppendLine("@startuml");

        foreach (var classInfo in _classMap.Values)
        {
            sb.Append($"{classInfo.ClassType} {classInfo.ClassName}");

            if (classInfo.Attributes.Count != 0 || classInfo.Methods.Count != 0)
            {
                sb.AppendLine(" {");

                foreach (var attr in classInfo.Attributes) sb.AppendLine($"  {attr}");

                foreach (var method in classInfo.Methods) sb.AppendLine($"  {method}");

                sb.AppendLine("}");
            }
            else
            {
                sb.AppendLine();
            }
        }

        sb.AppendLine("@enduml");

        var result = sb.ToString();
        result = StringHelper.NormalizeBreakLines(result);
        result = StringHelper.RemoveMultipleBreaks(result);
        result = StringHelper.NormalizeEndOfFile(result);

        return result;
    }

    public override string VisitClass_diagram(PumlgParser.Class_diagramContext context)
    {
        foreach (var classDecl in context.class_declaration()) VisitClass_declaration(classDecl);

        return string.Empty;
    }

    public override string VisitClass_declaration(PumlgParser.Class_declarationContext context)
    {
        var classType = context.class_type().GetText();
        var className = context.ident().GetText();


        if (!_classMap.ContainsKey(className))
            _classMap[className] = new ClassInfo
            {
                ClassType = classType,
                ClassName = className
            };

        var classInfo = _classMap[className];


        foreach (var attr in context.attribute())
        {
            var attrText = Visit(attr).TrimEnd();
            if (!classInfo.Attributes.Contains(attrText)) classInfo.Attributes.Add(attrText);
        }


        foreach (var method in context.method())
        {
            var methodText = Visit(method).TrimEnd();
            if (!classInfo.MethodSignatures.Add(methodText)) continue;
            classInfo.Methods.Add(methodText);
        }

        return string.Empty;
    }

    public override string VisitMethod(PumlgParser.MethodContext context)
    {
        var sb = new StringBuilder();

        if (context.visibility() != null)
            sb.Append(context.visibility().GetText());

        if (context.modifiers() != null)
            sb.Append(context.modifiers().GetText());

        if (context.type_declaration() != null)
        {
            sb.Append(' ');
            sb.Append(context.type_declaration().GetText());
        }

        if (context.visibility() != null || context.modifiers() != null || context.type_declaration() != null)
            sb.Append(' ');

        sb.Append(context.ident().GetText());

        sb.Append('(');
        if (context.function_argument_list() != null) sb.Append(Visit(context.function_argument_list()));
        sb.Append(')');

        return StringHelper.NormalizeBreakLines(sb.ToString());
    }

    public override string VisitAttribute(PumlgParser.AttributeContext context)
    {
        var sb = new StringBuilder();

        if (context.visibility() != null)
            sb.Append(context.visibility().GetText());

        if (context.modifiers() != null)
            sb.Append(context.modifiers().GetText());

        if (context.type_declaration() != null)
            sb.Append(" " + context.type_declaration().GetText());
        else
            sb.Append(' ');

        sb.Append(" " + context.ident().GetText());

        return StringHelper.NormalizeBreakLines(sb.ToString());
    }

    public override string VisitFunction_argument_list(PumlgParser.Function_argument_listContext context)
    {
        var args = context.function_argument().Select(Visit).ToList();

        return StringHelper.NormalizeBreakLines(string.Join(", ", args));
    }

    public override string VisitFunction_argument(PumlgParser.Function_argumentContext context)
    {
        var sb = new StringBuilder();

        if (context.type_declaration() != null)
            sb.Append(context.type_declaration().GetText() + " ");

        sb.Append(context.ident().GetText());

        return StringHelper.NormalizeBreakLines(sb.ToString());
    }

    private class ClassInfo
    {
        public string ClassType { get; init; } = string.Empty;
        public string ClassName { get; init; } = string.Empty;
        public List<string> Attributes { get; } = [];
        public List<string> Methods { get; } = [];
        public HashSet<string> MethodSignatures { get; } = [];
    }
}