using System.Collections.Generic;
using System.Linq;
using System.Text;
using PumlPlaner.Helpers;
using PumlPlaner.Visitors;

namespace PumlPlaner
{
    public class PlantUmlDeduplicator : PlantUmlReconstructor
    {
        public override string VisitUml(PumlgParser.UmlContext context)
        {
            var classDict = new Dictionary<string, ClassRepresentation>();

            foreach (var child in context.children)
            {
                if (child is PumlgParser.Class_declarationContext classCtx)
                {
                    var rep = ParseClass(classCtx);

                    if (classDict.TryGetValue(rep.Name, out var existing))
                    {
                        existing.Merge(rep);
                    }
                    else
                    {
                        classDict[rep.Name] = rep;
                    }
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("@startuml");

            foreach (var cls in classDict.Values)
            {
                sb.AppendLine(cls.ToPlantUml());
            }


            foreach (var child in context.children)
            {
                if (!(child is PumlgParser.Class_declarationContext))
                {
                    var other = Visit(child);
                    if (!string.IsNullOrWhiteSpace(other))
                        sb.AppendLine(other.TrimEnd());
                }
            }

            sb.AppendLine("@enduml");

            return new NormalizedInput(sb.ToString()).ToString();
        }

        private ClassRepresentation ParseClass(PumlgParser.Class_declarationContext ctx)
        {
            var rep = new ClassRepresentation(ctx.class_type().GetText(), ctx.ident().GetText());

            foreach (var attrCtx in ctx.attribute())
            {
                var attrRep = ParseAttribute(attrCtx);
                rep.AddAttribute(attrRep);
            }

            foreach (var methodCtx in ctx.method())
            {
                var methodRep = ParseMethod(methodCtx);
                rep.AddMethod(methodRep);
            }

            return rep;
        }

        private AttributeRepresentation ParseAttribute(PumlgParser.AttributeContext ctx)
        {
            var text = Visit(ctx).Trim();

            return new AttributeRepresentation(text);
        }

        private MethodRepresentation ParseMethod(PumlgParser.MethodContext ctx)
        {
            var methodName = ctx.ident().GetText();
            var parameters = new List<string>();

            if (ctx.function_argument_list() != null)
            {
                foreach (var arg in ctx.function_argument_list().function_argument())
                {
                    var typeText = arg.type_declaration()?.GetText() ?? "";
                    var identText = arg.ident().GetText();
                    parameters.Add($"{typeText} {identText}".Trim());
                }
            }

            var fullSignature = Visit(ctx).Trim();

            return new MethodRepresentation(methodName, parameters, fullSignature);
        }

        private class ClassRepresentation
        {
            public string Type { get; }
            public string Name { get; }
            private Dictionary<string, AttributeRepresentation> Attributes = new();
            private Dictionary<string, MethodRepresentation> Methods = new();

            public ClassRepresentation(string type, string name)
            {
                Type = type;
                Name = name;
            }

            public void AddAttribute(AttributeRepresentation attr)
            {
                if (!Attributes.ContainsKey(attr.Id))
                    Attributes[attr.Id] = attr;
            }

            public void AddMethod(MethodRepresentation method)
            {
                if (!Methods.ContainsKey(method.Id))
                    Methods[method.Id] = method;
            }

            public void Merge(ClassRepresentation other)
            {
                foreach (var attr in other.Attributes.Values)
                    AddAttribute(attr);
                foreach (var method in other.Methods.Values)
                    AddMethod(method);
            }

            public string ToPlantUml()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{Type} {Name} {{");

                foreach (var attr in Attributes.Values)
                    sb.AppendLine($"  {attr.Text}");
                foreach (var method in Methods.Values)
                    sb.AppendLine($"  {method.Text}");

                sb.AppendLine("}");
                return sb.ToString();
            }
        }

        private class AttributeRepresentation
        {
            public string Text { get; }
            public string Id { get; }

            public AttributeRepresentation(string text)
            {
                Text = text;
                Id = text;
            }
        }

        private class MethodRepresentation
        {
            public string Name { get; }
            public List<string> Parameters { get; }
            public string Text { get; }
            public string Id { get; }

            public MethodRepresentation(string name, List<string> parameters, string text)
            {
                Name = name;
                Parameters = parameters;
                Text = text;

                Id = GenerateId();
            }

            private string GenerateId()
            {
                var paramTypes = Parameters
                    .Select(p => p.Split(' ')[0])
                    .DefaultIfEmpty("")
                    .Aggregate((a, b) => a + "," + b);

                return $"{Name}({paramTypes})";
            }
        }
    }
}