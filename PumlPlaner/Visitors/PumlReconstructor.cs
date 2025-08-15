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


        return StringHelper.NormalizeBreakLines(sb.ToString());
    }

    public override string VisitClass_declaration(PumlgParser.Class_declarationContext context)
    {
        var sb = new StringBuilder();


        var classType = context.class_type().GetText();


        var className = context.ident().GetText();

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
}