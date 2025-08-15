using System.Text;
using PumlPlaner.Helpers;

namespace PumlPlaner.Visitors;

public class PumlReconstructor : PumlgBaseVisitor<string>
{
    public override string VisitUml(PumlgParser.UmlContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("@startuml");
        foreach (var child in context.children) sb.Append(Visit(child));

        sb.AppendLine("@enduml");

        return StringHelper.NormalizeBreakLines(sb.ToString());
    }

    public override string VisitClass_diagram(PumlgParser.Class_diagramContext context)
    {
        var sb = new StringBuilder();

        foreach (var classDecl in context.class_declaration()) sb.Append(Visit(classDecl));
        foreach (var enumDecl in context.enum_declaration()) sb.Append(Visit(enumDecl));
        foreach (var connection in context.connection()) sb.Append(Visit(connection));
        foreach (var hideDecl in context.hide_declaration()) sb.Append(Visit(hideDecl));

        return StringHelper.NormalizeBreakLines(sb.ToString());
    }

    public override string VisitClass_declaration(PumlgParser.Class_declarationContext context)
    {
        var sb = new StringBuilder();

        var classType = context.class_type().GetText();
        var className = context.ident().GetText();

        // Add space between abstract and class if needed
        if (classType == "abstractclass")
        {
            classType = "abstract class";
        }

        sb.Append($"{classType} {className}");

        if (context.ChildCount > 2)
        {
            sb.AppendLine(" {");

            foreach (var attr in context.attribute())
            {
                sb.Append("  ");
                sb.AppendLine(Visit(attr).TrimEnd());
            }

            foreach (var method in context.method())
            {
                sb.Append("  ");
                sb.AppendLine(Visit(method).TrimEnd());
            }

            sb.AppendLine("}");
        }
        else
        {
            sb.AppendLine();
        }

        return StringHelper.NormalizeBreakLines(sb.ToString());
    }

    public override string VisitEnum_declaration(PumlgParser.Enum_declarationContext context)
    {
        var sb = new StringBuilder();

        var enumName = context.ident().GetText();
        sb.Append($"enum {enumName}");

        if (context.item_list() != null)
        {
            sb.AppendLine(" {");
            sb.Append(Visit(context.item_list()));
            sb.AppendLine("}");
        }
        else
        {
            sb.AppendLine();
        }

        return StringHelper.NormalizeBreakLines(sb.ToString());
    }

    public override string VisitItem_list(PumlgParser.Item_listContext context)
    {
        var sb = new StringBuilder();

        foreach (var item in context.ident())
        {
            sb.AppendLine($"  {item.GetText()}");
        }

        return StringHelper.NormalizeBreakLines(sb.ToString());
    }

    public override string VisitConnection(PumlgParser.ConnectionContext context)
    {
        var sb = new StringBuilder();

        // Left side
        if (context.left != null)
            sb.Append(Visit(context.left));

        // Connector
        if (context.CONNECTOR() != null)
            sb.Append($" {context.CONNECTOR().GetText()} ");

        // Right side
        if (context.right != null)
            sb.Append(Visit(context.right));

        // Stereotype
        if (context.stereotype() != null)
        {
            sb.Append($" : {Visit(context.stereotype())}");
        }

        sb.AppendLine();

        return StringHelper.NormalizeBreakLines(sb.ToString());
    }

    public override string VisitConnection_left(PumlgParser.Connection_leftContext context)
    {
        var sb = new StringBuilder();

        sb.Append(context.instance.GetText());

        if (context.attrib != null)
        {
            sb.Append($" \"{context.attrib.GetText()}\"");
            if (context.mult != null)
            {
                sb.Append(context.mult.GetText());
            }
        }

        return StringHelper.NormalizeBreakLines(sb.ToString());
    }

    public override string VisitConnection_right(PumlgParser.Connection_rightContext context)
    {
        var sb = new StringBuilder();

        if (context.attrib != null)
        {
            sb.Append($"\"{context.attrib.GetText()}\"");
            if (context.mult != null)
            {
                sb.Append(context.mult.GetText());
            }
            sb.Append(" ");
        }

        sb.Append(context.instance.GetText());

        return StringHelper.NormalizeBreakLines(sb.ToString());
    }

    public override string VisitStereotype(PumlgParser.StereotypeContext context)
    {
        var sb = new StringBuilder();

        sb.Append($"<<{context.name.GetText()}");

        if (context._args != null && context._args.Count > 0)
        {
            sb.Append("(");
            sb.Append(string.Join(", ", context._args.Select(arg => arg.GetText())));
            sb.Append(")");
        }

        sb.Append(">>");

        return StringHelper.NormalizeBreakLines(sb.ToString());
    }

    public override string VisitHide_declaration(PumlgParser.Hide_declarationContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"hide {context.ident().GetText()}");

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

    public override string VisitTemplate_type(PumlgParser.Template_typeContext context)
    {
        var sb = new StringBuilder();

        sb.Append(context.ident().GetText());
        sb.Append('<');

        if (context.template_argument_list() != null)
        {
            sb.Append(Visit(context.template_argument_list()));
        }

        sb.Append('>');

        return StringHelper.NormalizeBreakLines(sb.ToString());
    }

    public override string VisitList_type(PumlgParser.List_typeContext context)
    {
        var sb = new StringBuilder();

        sb.Append(context.ident().GetText());
        sb.Append("[]");

        return StringHelper.NormalizeBreakLines(sb.ToString());
    }

    public override string VisitSimple_type(PumlgParser.Simple_typeContext context)
    {
        return StringHelper.NormalizeBreakLines(context.ident().GetText());
    }

    public override string VisitTemplate_argument_list(PumlgParser.Template_argument_listContext context)
    {
        var args = context.template_argument().Select(Visit).ToList();

        return StringHelper.NormalizeBreakLines(string.Join(", ", args));
    }

    public override string VisitTemplate_argument(PumlgParser.Template_argumentContext context)
    {
        return StringHelper.NormalizeBreakLines(Visit(context.type_declaration()));
    }

    public override string VisitModifiers(PumlgParser.ModifiersContext context)
    {
        return StringHelper.NormalizeBreakLines(context.GetText());
    }

    public override string VisitMultiplicity(PumlgParser.MultiplicityContext context)
    {
        return StringHelper.NormalizeBreakLines(context.GetText());
    }
}
