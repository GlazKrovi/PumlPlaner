using System.Text;
using PumlPlaner.Helpers;

namespace PumlPlaner.Visitors;

public class PumlSum : PumlReconstructor
{
    private readonly Dictionary<string, ClassInfo> _classMap = new();
    private readonly Dictionary<string, EnumInfo> _enumMap = new();
    private readonly List<string> _connections = [];
    private readonly List<string> _hideDeclarations = [];

    public string VisitUml(params PumlgParser.UmlContext[] contexts)
    {
        _classMap.Clear();
        _enumMap.Clear();
        _connections.Clear();
        _hideDeclarations.Clear();


        foreach (var ctx in contexts)
        {
            if (ctx.class_diagram() != null)
            {
                VisitClass_diagram(ctx.class_diagram());
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("@startuml");


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


        foreach (var classInfo in _classMap.Values)
        {
            sb.Append($"{classInfo.ClassType} {classInfo.ClassName}");
            
            if (!string.IsNullOrEmpty(classInfo.TemplateParameters))
            {
                sb.Append(classInfo.TemplateParameters);
            }
            
            if (!string.IsNullOrEmpty(classInfo.Stereotype))
            {
                sb.Append(" " + classInfo.Stereotype);
            }


            if (!string.IsNullOrEmpty(classInfo.Extends) || classInfo.Implements.Count > 0)
            {
                if (!string.IsNullOrEmpty(classInfo.Extends))
                {
                    sb.Append($" extends {classInfo.Extends}");
                }

                if (classInfo.Implements.Count > 0)
                {
                    sb.Append(" implements ");
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


        foreach (var connection in _connections)
        {
            sb.AppendLine(connection);
        }


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


        if (classType == "abstractclass")
        {
            classType = "abstract class";
        }

        if (!_classMap.TryGetValue(className, out var value))
        {
            value = new ClassInfo
            {
                ClassType = classType,
                ClassName = className,
                Attributes = [],
                Methods = []
            };
            _classMap[className] = value;


            if (context.template_parameter_list() != null)
            {
                _classMap[className].TemplateParameters = Visit(context.template_parameter_list());
            }


            if (context.stereotype() != null)
            {
                _classMap[className].Stereotype = Visit(context.stereotype());
            }
        }


        if (context.inheritance_declaration() != null)
        {
            VisitInheritance_declaration(context.inheritance_declaration(), value);
        }


        foreach (var member in context.class_member())
        {
            var memberText = Visit(member).Trim();
            if (string.IsNullOrWhiteSpace(memberText)) continue;
            if (member.attribute() != null && !_classMap[className].Attributes.Contains(memberText))
            {
                _classMap[className].Attributes.Add(memberText);
            }
            else if (member.method() != null && !_classMap[className].Methods.Contains(memberText))
            {
                _classMap[className].Methods.Add(memberText);
            }
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
                EnumName = enumName,
                Items = []
            };
        }

        if (context.item_list() == null) return string.Empty;
        foreach (var item in context.item_list().ident())
        {
            var itemText = item.GetText();
            if (!_enumMap[enumName].Items.Contains(itemText))
            {
                _enumMap[enumName].Items.Add(itemText);
            }
        }

        return string.Empty;
    }

    public override string VisitConnection(PumlgParser.ConnectionContext context)
    {
        var connectionText = Visit(context.left) + " " + context.CONNECTOR().GetText() + " " + Visit(context.right);
        
        if (context.stereotype() != null)
        {
            connectionText += " : " + Visit(context.stereotype());
        }

        if (!_connections.Contains(connectionText))
        {
            _connections.Add(connectionText);
        }

        return string.Empty;
    }

    public override string VisitHide_declaration(PumlgParser.Hide_declarationContext context)
    {
        var hideText = "hide " + context.ident().GetText();
        
        if (!_hideDeclarations.Contains(hideText))
        {
            _hideDeclarations.Add(hideText);
        }

        return string.Empty;
    }

    private void VisitInheritance_declaration(PumlgParser.Inheritance_declarationContext context, ClassInfo classInfo)
    {
        if (context.extends_declaration() != null)
        {
            classInfo.Extends = context.extends_declaration().ident().GetText();
        }

        if (context.implements_declaration() == null) return;
        foreach (var ident in context.implements_declaration().ident())
        {
            var interfaceName = ident.GetText();
            if (!classInfo.Implements.Contains(interfaceName))
            {
                classInfo.Implements.Add(interfaceName);
            }
        }
    }

    private class ClassInfo
    {
        public string ClassType { get; init; } = string.Empty;
        public string ClassName { get; init; } = string.Empty;
        public List<string> Attributes { get; init; } = [];
        public List<string> Methods { get; init; } = [];
        public string? TemplateParameters { get; set; }
        public string? Stereotype { get; set; }
        public string? Extends { get; set; }
        public List<string> Implements { get; set; } = [];
    }

    private class EnumInfo
    {
        public string EnumName { get; init; } = string.Empty;
        public List<string> Items { get; init; } = [];
    }
}
