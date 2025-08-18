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

            // Add inheritance declarations
            if (!string.IsNullOrEmpty(classInfo.Extends) || classInfo.Implements.Count > 0)
            {
                if (!string.IsNullOrEmpty(classInfo.Extends))
                {
                    sb.Append($" extends {classInfo.Extends}");
                }

                if (classInfo.Implements.Count > 0)
                {
                    if (!string.IsNullOrEmpty(classInfo.Extends))
                    {
                        sb.Append(" implements ");
                    }
                    else
                    {
                        sb.Append(" implements ");
                    }
                    sb.Append(string.Join(", ", classInfo.Implements));
                }
            }

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

        // Handle inheritance declarations
        if (context.inheritance_declaration() != null)
        {
            VisitInheritance_declaration(context.inheritance_declaration(), classInfo);
        }

        foreach (var member in context.class_member())
        {
            var memberText = base.VisitClass_member(member).TrimEnd();
            
            // Déterminer si c'est un attribut ou une méthode
            if (member.attribute() != null)
            {
                if (!classInfo.Attributes.Contains(memberText)) classInfo.Attributes.Add(memberText);
            }
            else if (member.method() != null)
            {
                if (!classInfo.MethodSignatures.Add(memberText)) continue;
                classInfo.Methods.Add(memberText);
            }
        }

        return string.Empty;
    }

    private void VisitInheritance_declaration(PumlgParser.Inheritance_declarationContext context, ClassInfo classInfo)
    {
        if (context.extends_declaration() != null)
        {
            classInfo.Extends = context.extends_declaration().ident().GetText();
        }

        if (context.implements_declaration() != null)
        {
            foreach (var ident in context.implements_declaration().ident())
            {
                var interfaceName = ident.GetText();
                if (!classInfo.Implements.Contains(interfaceName))
                {
                    classInfo.Implements.Add(interfaceName);
                }
            }
        }
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
            sb.Append(" " + context.modifiers().GetText());

        // Gérer les deux syntaxes : type ident() ou ident() : type
        if (context.type_declaration() != null)
        {
            // Si le type est avant l'identifiant (ancienne syntaxe)
            if (context.type_declaration().Start.TokenIndex < context.ident().Start.TokenIndex)
            {
                sb.Append(" " + Visit(context.type_declaration()));
                sb.Append(" " + context.ident().GetText());
            }
            // Si le type est après l'identifiant avec : (nouvelle syntaxe)
            else
            {
                sb.Append(" " + context.ident().GetText());
            }
        }
        else
        {
            sb.Append(" " + context.ident().GetText());
        }

        sb.Append('(');
        if (context.function_argument_list() != null) sb.Append(Visit(context.function_argument_list()));
        sb.Append(')');

        if (context.type_declaration() != null)
        {
            // Si le type est après l'identifiant avec : (nouvelle syntaxe)
            if (context.type_declaration().Start.TokenIndex > context.ident().Start.TokenIndex)
            {
                sb.Append(" : ");
                sb.Append(Visit(context.type_declaration()));
            }
        }

        return StringHelper.NormalizeBreakLines(sb.ToString());
    }

    public override string VisitAttribute(PumlgParser.AttributeContext context)
    {
        var sb = new StringBuilder();

        if (context.visibility() != null)
            sb.Append(context.visibility().GetText());

        if (context.modifiers() != null)
            sb.Append(" " + context.modifiers().GetText());

        // Gérer les deux syntaxes : type ident ou ident : type
        if (context.type_declaration() != null)
        {
            // Si le type est avant l'identifiant (ancienne syntaxe)
            if (context.type_declaration().Start.TokenIndex < context.ident().Start.TokenIndex)
            {
                sb.Append(" " + Visit(context.type_declaration()));
                sb.Append(" " + context.ident().GetText());
            }
            // Si le type est après l'identifiant avec : (nouvelle syntaxe)
            else
            {
                sb.Append(" " + context.ident().GetText());
                sb.Append(" : ");
                sb.Append(Visit(context.type_declaration()));
            }
        }
        else
        {
            sb.Append(" " + context.ident().GetText());
        }

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
        public string? Extends { get; set; }
        public List<string> Implements { get; } = [];
    }

    private class EnumInfo
    {
        public string EnumName { get; init; } = string.Empty;
        public List<string> Items { get; } = [];
    }
}
