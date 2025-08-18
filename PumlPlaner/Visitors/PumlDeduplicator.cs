using System.Text;
using PumlPlaner.Helpers;

namespace PumlPlaner.Visitors;

public class PumlDeduplicator : PumlReconstructor
{
    private readonly Dictionary<string, ClassInfo> _classMap = new();
    private readonly Dictionary<string, EnumInfo> _enumMap = new();
    private readonly List<string> _connections = new();
    private readonly List<string> _hideDeclarations = new();

    public override string VisitUml(PumlgParser.UmlContext context)
    {
        _classMap.Clear();
        _enumMap.Clear();
        _connections.Clear();
        _hideDeclarations.Clear();

        if (context.class_diagram() != null) VisitClass_diagram(context.class_diagram());

        var sb = new StringBuilder();
        sb.AppendLine("@startuml");

        // Output enums
        foreach (var enumInfo in _enumMap.Values)
        {
            sb.Append($"enum {enumInfo.EnumName}");
            if (enumInfo.Items.Count > 0)
            {
                sb.AppendLine(" {");
                foreach (var item in enumInfo.Items)
                {
                    sb.AppendLine($"  {item}");
                }
                sb.AppendLine("}");
            }
            else
            {
                sb.AppendLine();
            }
        }

        // Output classes
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

        // Output connections
        foreach (var connection in _connections)
        {
            sb.AppendLine(connection);
        }

        // Output hide declarations
        foreach (var hideDecl in _hideDeclarations)
        {
            sb.AppendLine(hideDecl);
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
        foreach (var enumDecl in context.enum_declaration()) VisitEnum_declaration(enumDecl);
        foreach (var connection in context.connection()) VisitConnection(connection);
        foreach (var hideDecl in context.hide_declaration()) VisitHide_declaration(hideDecl);

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
            // Utiliser la méthode du parent (PumlReconstructor) pour préserver toutes les caractéristiques
            var attrText = base.VisitAttribute(attr).TrimEnd();
            if (!classInfo.Attributes.Contains(attrText)) classInfo.Attributes.Add(attrText);
        }

        foreach (var method in context.method())
        {
            // Utiliser la méthode du parent (PumlReconstructor) pour préserver toutes les caractéristiques
            var methodText = base.VisitMethod(method).TrimEnd();
            if (!classInfo.MethodSignatures.Add(methodText)) continue;
            classInfo.Methods.Add(methodText);
        }

        return string.Empty;
    }

    public override string VisitEnum_declaration(PumlgParser.Enum_declarationContext context)
    {
        var enumName = context.ident().GetText();

        if (!_enumMap.ContainsKey(enumName))
        {
            _enumMap[enumName] = new EnumInfo
            {
                EnumName = enumName
            };
        }

        var enumInfo = _enumMap[enumName];

        if (context.item_list() != null)
        {
            foreach (var item in context.item_list().ident())
            {
                var itemText = item.GetText();
                if (!enumInfo.Items.Contains(itemText))
                {
                    enumInfo.Items.Add(itemText);
                }
            }
        }

        return string.Empty;
    }

    public override string VisitConnection(PumlgParser.ConnectionContext context)
    {
        var connectionText = base.VisitConnection(context).TrimEnd();
        if (!_connections.Contains(connectionText))
        {
            _connections.Add(connectionText);
        }

        return string.Empty;
    }

    public override string VisitHide_declaration(PumlgParser.Hide_declarationContext context)
    {
        var hideText = base.VisitHide_declaration(context).TrimEnd();
        if (!_hideDeclarations.Contains(hideText))
        {
            _hideDeclarations.Add(hideText);
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

    private class EnumInfo
    {
        public string EnumName { get; init; } = string.Empty;
        public List<string> Items { get; } = [];
    }
}
