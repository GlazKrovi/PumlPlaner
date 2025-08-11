using System.Text;

namespace PumlPlaner.Visitors;

public class PlantUmlReconstructor : PumlgBaseVisitor<string>
{
    public override string VisitUml(PumlgParser.UmlContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("@startuml");
        foreach (var child in context.children)
        {
            sb.Append(Visit(child));
        }
        sb.AppendLine("@enduml");

        return sb.ToString();
    }

    public override string VisitClass_diagram(PumlgParser.Class_diagramContext context)
    {
        var sb = new StringBuilder();

        foreach (var classDecl in context.class_declaration())
        {
            sb.Append(Visit(classDecl));
        }

        // Ajoute ici d'autres visites si besoin (connections, enums...)

        return sb.ToString();
    }

    public override string VisitClass_declaration(PumlgParser.Class_declarationContext context)
    {
        var sb = new StringBuilder();

        // Type de la classe : "class", "abstract", "interface"...
        var classType = context.class_type().GetText();

        // Nom de la classe (ident)
        var className = context.ident().GetText();

        sb.Append($"{classType} {className}");

        // Si la classe a un corps (attributs/méthodes)
        if (context.ChildCount > 2) // typiquement { ... }
        {
            sb.AppendLine(" {");

            // Attributs
            foreach (var attr in context.attribute())
            {
                sb.Append("  ");
                sb.AppendLine(Visit(attr).TrimEnd());
            }

            // Méthodes
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

        return sb.ToString();
    }

    public override string VisitAttribute(PumlgParser.AttributeContext context)
    {
        var sb = new StringBuilder();

        // Visibilité (+, -, #)
        if (context.visibility() != null)
            sb.Append(context.visibility().GetText());

        // Modifiers (static, abstract)
        if (context.modifiers() != null)
            sb.Append(context.modifiers().GetText());

        // Type
        if (context.type_declaration() != null)
            sb.Append(" " + context.type_declaration().GetText());
        else
            sb.Append(" ");

        // Nom de l'attribut
        sb.Append(" " + context.ident().GetText());

        return sb.ToString();
    }

    public override string VisitMethod(PumlgParser.MethodContext context)
    {
        var sb = new StringBuilder();

        // Visibilité (+, -, #)
        if (context.visibility() != null)
            sb.Append(context.visibility().GetText());

        // Modifiers (static, abstract)
        if (context.modifiers() != null)
            sb.Append(context.modifiers().GetText());

        // Type de retour
        if (context.type_declaration() != null)
            sb.Append(" " + context.type_declaration().GetText());
        else
            sb.Append(" ");

        // Nom de la méthode
        sb.Append(" " + context.ident().GetText());

        // Arguments
        sb.Append("(");
        if (context.function_argument_list() != null)
        {
            sb.Append(Visit(context.function_argument_list()));
        }
        sb.Append(")");

        return sb.ToString();
    }

    public override string VisitFunction_argument_list(PumlgParser.Function_argument_listContext context)
    {
        var args = new List<string>();
        foreach (var arg in context.function_argument())
        {
            args.Add(Visit(arg));
        }
        return string.Join(", ", args);
    }

    public override string VisitFunction_argument(PumlgParser.Function_argumentContext context)
    {
        var sb = new StringBuilder();

        if (context.type_declaration() != null)
            sb.Append(context.type_declaration().GetText() + " ");

        sb.Append(context.ident().GetText());

        return sb.ToString();
    }
}
