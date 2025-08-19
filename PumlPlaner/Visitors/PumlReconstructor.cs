using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PumlPlaner.Helpers;

namespace PumlPlaner.Visitors;

/// <summary>
///     Exception personnalisée pour les erreurs de reconstruction PUML
/// </summary>
public class PumlReconstructionException : Exception
{
    public PumlReconstructionException(string message) : base(message)
    {
    }

    public PumlReconstructionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class PumlReconstructor : PumlgBaseVisitor<string>
{
    private readonly List<string> _errors = new();
    private readonly bool _throwOnError;

    public PumlReconstructor(bool throwOnError = false)
    {
        _throwOnError = throwOnError;
    }

    /// <summary>
    ///     Récupère les erreurs collectées pendant la reconstruction
    /// </summary>
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();

    /// <summary>
    ///     Indique s'il y a eu des erreurs pendant la reconstruction
    /// </summary>
    public bool HasErrors => _errors.Count > 0;

    /// <summary>
    ///     Ajoute une erreur à la liste et gère selon la configuration
    /// </summary>
    private void AddError(string error, Exception? exception = null)
    {
        var errorMessage = exception != null ? $"{error}: {exception.Message}" : error;
        _errors.Add(errorMessage);

        if (_throwOnError) throw new PumlReconstructionException(errorMessage, exception);
    }

    /// <summary>
    ///     Méthode sécurisée pour visiter un nœud avec gestion d'erreurs
    /// </summary>
    private string SafeVisit(IParseTree? node, string context = "node")
    {
        if (node == null) return string.Empty;

        try
        {
            return Visit(node);
        }
        catch (RecognitionException ex)
        {
            // Erreur de syntaxe ANTLR - on l'ajoute à la liste des erreurs
            AddError($"Erreur de syntaxe dans {context}", ex);
            return string.Empty;
        }
        catch (Exception ex)
        {
            // Autres erreurs - on les ignore silencieusement pour les petits nœuds
            if (node.ChildCount <= 2) return string.Empty;

            AddError($"Erreur lors de la visite de {context}", ex);
            return string.Empty;
        }
    }

    public override string VisitUml(PumlgParser.UmlContext context)
    {
        if (context == null)
        {
            AddError("Contexte UML invalide");
            return string.Empty;
        }

        var sb = new StringBuilder();

        try
        {
            sb.AppendLine("@startuml");

            foreach (var child in context.children) sb.Append(SafeVisit(child, "élément UML"));

            sb.AppendLine("@enduml");

            return new NormalizedInput(sb.ToString()).ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction du diagramme UML", ex);
            return sb.ToString();
        }
    }

    public override string VisitClass_diagram(PumlgParser.Class_diagramContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            foreach (var classDecl in context.class_declaration())
                sb.Append(SafeVisit(classDecl, "déclaration de classe"));

            foreach (var enumDecl in context.enum_declaration())
                sb.Append(SafeVisit(enumDecl, "déclaration d'énumération"));

            foreach (var connection in context.connection())
                sb.Append(SafeVisit(connection, "connexion"));

            foreach (var hideDecl in context.hide_declaration())
                sb.Append(SafeVisit(hideDecl, "déclaration hide"));

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction du diagramme de classes", ex);
            return sb.ToString();
        }
    }

    public override string VisitClass_declaration(PumlgParser.Class_declarationContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            var classType = context.class_type()?.GetText() ?? "class";
            var className = context.ident()?.GetText() ?? "UnknownClass";

            if (classType == "abstractclass") classType = "abstract class";

            sb.Append($"{classType} {className}");

            if (context.template_parameter_list() != null)
                sb.Append(SafeVisit(context.template_parameter_list(), "paramètres de template"));

            if (context.stereotype() != null)
            {
                sb.Append(' ');
                sb.Append(SafeVisit(context.stereotype(), "stéréotype"));
            }

            if (context.inheritance_declaration() != null)
            {
                sb.Append(' ');
                sb.Append(SafeVisit(context.inheritance_declaration(), "déclaration d'héritage"));
            }

            if (context.ChildCount > 2)
            {
                sb.AppendLine(" {");

                var members = context.class_member().ToList();

                for (var i = 0; i < members.Count; i++)
                {
                    sb.Append("  ");
                    sb.Append(SafeVisit(members[i], "membre de classe").TrimEnd());
                    if (i < members.Count - 1) sb.AppendLine();
                }

                sb.AppendLine("\n}");
            }
            else
            {
                sb.AppendLine();
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de la déclaration de classe", ex);
            return sb.ToString();
        }
    }

    public override string VisitEnum_declaration(PumlgParser.Enum_declarationContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            var enumName = context.ident()?.GetText() ?? "UnknownEnum";
            sb.Append($"enum {enumName}");

            if (context.item_list() != null)
            {
                sb.AppendLine(" {");
                sb.Append(SafeVisit(context.item_list(), "liste d'éléments"));
                sb.AppendLine("}");
            }
            else
            {
                sb.AppendLine();
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de la déclaration d'énumération", ex);
            return sb.ToString();
        }
    }

    public override string VisitInheritance_declaration(PumlgParser.Inheritance_declarationContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            if (context.extends_declaration() != null)
                sb.Append(SafeVisit(context.extends_declaration(), "déclaration extends"));

            if (context.implements_declaration() == null) return sb.ToString();

            if (context.extends_declaration() != null) sb.Append(' ');
            sb.Append(SafeVisit(context.implements_declaration(), "déclaration implements"));

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de la déclaration d'héritage", ex);
            return sb.ToString();
        }
    }

    public override string VisitExtends_declaration(PumlgParser.Extends_declarationContext context)
    {
        if (context?.ident() == null) return string.Empty;

        try
        {
            return $"extends {context.ident().GetText()}";
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de la déclaration extends", ex);
            return string.Empty;
        }
    }

    public override string VisitImplements_declaration(PumlgParser.Implements_declarationContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            sb.Append("implements ");

            var identifiers = context.ident().ToList();
            for (var i = 0; i < identifiers.Count; i++)
            {
                sb.Append(identifiers[i].GetText());
                if (i < identifiers.Count - 1) sb.Append(", ");
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de la déclaration implements", ex);
            return sb.ToString();
        }
    }

    public override string VisitItem_list(PumlgParser.Item_listContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            foreach (var item in context.ident())
                sb.AppendLine($"  {item.GetText()}");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de la liste d'éléments", ex);
            return sb.ToString();
        }
    }

    public override string VisitConnection(PumlgParser.ConnectionContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            if (context.left != null)
                sb.Append(SafeVisit(context.left, "connexion gauche"));

            if (context.CONNECTOR() != null)
                sb.Append($" {context.CONNECTOR().GetText()} ");

            if (context.right != null)
                sb.Append(SafeVisit(context.right, "connexion droite"));

            if (context.stereotype() != null)
                sb.Append($" : {SafeVisit(context.stereotype(), "stéréotype de connexion")}");

            sb.AppendLine();

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de la connexion", ex);
            return sb.ToString();
        }
    }

    public override string VisitConnection_left(PumlgParser.Connection_leftContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            sb.Append(context.instance?.GetText() ?? "Unknown");

            if (context.attrib == null) return sb.ToString();

            sb.Append($" \"{context.attrib.GetText()}\"");
            if (context.mult != null) sb.Append(context.mult.GetText());

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de la connexion gauche", ex);
            return sb.ToString();
        }
    }

    public override string VisitConnection_right(PumlgParser.Connection_rightContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            if (context.attrib != null)
            {
                sb.Append($"\"{context.attrib.GetText()}\"");
                if (context.mult != null) sb.Append(context.mult.GetText());
                sb.Append(' ');
            }

            sb.Append(context.instance?.GetText() ?? "Unknown");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de la connexion droite", ex);
            return sb.ToString();
        }
    }

    public override string VisitStereotype(PumlgParser.StereotypeContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            sb.Append($"<<{context.name?.GetText() ?? "Unknown"}");

            if (context._args is { Count: > 0 })
            {
                sb.Append('(');
                sb.Append(string.Join(", ", context._args.Select(arg => arg.GetText())));
                sb.Append(')');
            }

            sb.Append(">>");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction du stéréotype", ex);
            return sb.ToString();
        }
    }

    public override string VisitHide_declaration(PumlgParser.Hide_declarationContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            sb.AppendLine($"hide {context.ident()?.GetText() ?? "Unknown"}");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de la déclaration hide", ex);
            return sb.ToString();
        }
    }

    public override string VisitAttribute(PumlgParser.AttributeContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            if (context.visibility() != null)
                sb.Append(context.visibility().GetText());

            if (context.modifiers() != null)
                sb.Append(" " + context.modifiers().GetText());

            if (context.type_declaration() != null)
            {
                if (context.type_declaration().Start.TokenIndex < context.ident().Start.TokenIndex)
                {
                    sb.Append(" " + SafeVisit(context.type_declaration(), "type d'attribut"));
                    sb.Append(" " + context.ident().GetText());
                }
                else
                {
                    sb.Append(" " + context.ident().GetText());
                    sb.Append(" : ");
                    sb.Append(SafeVisit(context.type_declaration(), "type d'attribut"));
                }
            }
            else
            {
                sb.Append(" " + context.ident().GetText());
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de l'attribut", ex);
            return sb.ToString();
        }
    }

    public override string VisitMethod(PumlgParser.MethodContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            if (context.visibility() != null)
                sb.Append(context.visibility().GetText());

            if (context.modifiers() != null)
                sb.Append(" " + context.modifiers().GetText());

            if (context.ident() == null) return sb.ToString();

            if (context.type_declaration() != null)
            {
                if (context.type_declaration().Start.TokenIndex < context.ident().Start.TokenIndex)
                {
                    sb.Append(" " + SafeVisit(context.type_declaration(), "type de méthode"));
                    sb.Append(" " + context.ident().GetText());
                }
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
            if (context.function_argument_list() != null)
                sb.Append(SafeVisit(context.function_argument_list(), "arguments de fonction"));
            sb.Append(')');

            if (context.type_declaration() == null ||
                context.type_declaration().Start.TokenIndex <= context.ident().Start.TokenIndex)
                return sb.ToString();

            sb.Append(" : ");
            sb.Append(SafeVisit(context.type_declaration(), "type de retour"));

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de la méthode", ex);
            return sb.ToString();
        }
    }

    public override string VisitFunction_argument_list(PumlgParser.Function_argument_listContext context)
    {
        if (context == null) return string.Empty;

        try
        {
            var args = context.function_argument().Select(arg => SafeVisit(arg, "argument de fonction")).ToList();
            return string.Join(", ", args);
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de la liste d'arguments", ex);
            return string.Empty;
        }
    }

    public override string VisitFunction_argument(PumlgParser.Function_argumentContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            if (context.type_declaration() != null)
                sb.Append(context.type_declaration().GetText() + " ");

            sb.Append(context.ident().GetText());

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de l'argument de fonction", ex);
            return sb.ToString();
        }
    }

    public override string VisitTemplate_type(PumlgParser.Template_typeContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            sb.Append(context.ident().GetText());
            sb.Append('<');

            if (context.template_argument_list() != null)
                sb.Append(SafeVisit(context.template_argument_list(), "arguments de template"));

            sb.Append('>');

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction du type template", ex);
            return sb.ToString();
        }
    }

    public override string VisitList_type(PumlgParser.List_typeContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            sb.Append(context.ident().GetText());
            sb.Append("[]");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction du type liste", ex);
            return sb.ToString();
        }
    }

    public override string VisitSimple_type(PumlgParser.Simple_typeContext context)
    {
        if (context == null) return string.Empty;

        try
        {
            return context.ident().GetText();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction du type simple", ex);
            return string.Empty;
        }
    }

    public override string VisitGeneric_list_type(PumlgParser.Generic_list_typeContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            sb.Append(context.template_parameter().GetText());
            sb.Append("[]");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction du type liste générique", ex);
            return sb.ToString();
        }
    }

    public override string VisitGeneric_simple_type(PumlgParser.Generic_simple_typeContext context)
    {
        if (context == null) return string.Empty;

        try
        {
            return context.template_parameter().GetText();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction du type simple générique", ex);
            return string.Empty;
        }
    }

    public override string VisitTemplate_argument_list(PumlgParser.Template_argument_listContext context)
    {
        if (context == null) return string.Empty;

        try
        {
            var args = context.template_argument().Select(arg => SafeVisit(arg, "argument de template")).ToList();
            return string.Join(", ", args);
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de la liste d'arguments de template", ex);
            return string.Empty;
        }
    }

    public override string VisitTemplate_argument(PumlgParser.Template_argumentContext context)
    {
        if (context == null) return string.Empty;

        try
        {
            return SafeVisit(context.type_declaration(), "argument de template");
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de l'argument de template", ex);
            return string.Empty;
        }
    }

    public override string VisitTemplate_parameter_list(PumlgParser.Template_parameter_listContext context)
    {
        if (context == null) return string.Empty;

        var sb = new StringBuilder();

        try
        {
            sb.Append('<');
            var parameters = context.template_parameter().Select(param => SafeVisit(param, "paramètre de template"))
                .ToList();
            sb.Append(string.Join(", ", parameters));
            sb.Append('>');

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de la liste de paramètres de template", ex);
            return sb.ToString();
        }
    }

    public override string VisitTemplate_parameter(PumlgParser.Template_parameterContext context)
    {
        if (context == null) return string.Empty;

        try
        {
            return context.ident().GetText();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction du paramètre de template", ex);
            return string.Empty;
        }
    }

    public override string VisitClass_member(PumlgParser.Class_memberContext context)
    {
        if (context == null) return string.Empty;

        try
        {
            if (context.attribute() != null)
                return SafeVisit(context.attribute(), "attribut");
            if (context.method() != null)
                return SafeVisit(context.method(), "méthode");
            return string.Empty;
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction du membre de classe", ex);
            return string.Empty;
        }
    }

    public override string VisitModifiers(PumlgParser.ModifiersContext context)
    {
        if (context == null) return string.Empty;

        try
        {
            return context.GetText();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction des modificateurs", ex);
            return string.Empty;
        }
    }

    public override string VisitMultiplicity(PumlgParser.MultiplicityContext context)
    {
        if (context == null) return string.Empty;

        try
        {
            return context.GetText();
        }
        catch (Exception ex)
        {
            AddError("Erreur lors de la reconstruction de la multiplicité", ex);
            return string.Empty;
        }
    }
}