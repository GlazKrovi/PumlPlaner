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
            // Collecter les classes
            var classes = new Dictionary<string, ClassRepresentation>();

            // Visiter les enfants et collecter les classes
            foreach (var child in context.children)
            {
                if (child is PumlgParser.Class_declarationContext classCtx)
                {
                    var rep = ParseClass(classCtx);

                    if (classes.TryGetValue(rep.Name, out var existing))
                    {
                        existing.Merge(rep);
                    }
                    else
                    {
                        classes[rep.Name] = rep;
                    }
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("@startuml");

            // Ajouter toutes les classes fusionnées
            foreach (var c in classes.Values)
            {
                sb.AppendLine(c.ToPlantUml());
            }

            // Ici, si tu veux gérer aussi d’autres éléments du schéma (enums, connexions...),
            // il faudrait aussi les visiter et les ajouter, par exemple :

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

            return StringHelper.NormalizeBreakLines(sb.ToString());
        }

        private ClassRepresentation ParseClass(PumlgParser.Class_declarationContext ctx)
        {
            var rep = new ClassRepresentation
            {
                Name = ctx.ident().GetText(),
                Type = ctx.class_type().GetText()
            };

            foreach (var attrCtx in ctx.attribute())
            {
                var attr = Visit(attrCtx).Trim();
                rep.Attributes.Add(attr);
            }

            foreach (var mCtx in ctx.method())
            {
                var methodSignature = Visit(mCtx).Trim();
                rep.AddMethodFromSignature(methodSignature);
            }

            return rep;
        }

        private class ClassRepresentation
        {
            public string Name;
            public string Type;
            public HashSet<string> Attributes = new HashSet<string>();
            private Dictionary<string, string> MethodsBySignature = new Dictionary<string, string>();

            public void AddMethodFromSignature(string methodSignature)
            {
                var key = GenerateMethodKey(methodSignature);
                if (!MethodsBySignature.ContainsKey(key))
                    MethodsBySignature[key] = methodSignature;
            }

            private string GenerateMethodKey(string methodSignature)
            {
                var idxStartParams = methodSignature.IndexOf('(');
                var idxEndParams = methodSignature.IndexOf(')');
                if (idxStartParams < 0 || idxEndParams < 0) return methodSignature;

                var methodName = methodSignature.Substring(0, idxStartParams).Trim();
                var parameters = methodSignature.Substring(idxStartParams + 1, idxEndParams - idxStartParams - 1).Trim();

                return $"{methodName}({parameters})";
            }

            public void Merge(ClassRepresentation other)
            {
                Attributes.UnionWith(other.Attributes);
                foreach (var m in other.MethodsBySignature.Values)
                    AddMethodFromSignature(m);
            }

            public string ToPlantUml()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{Type} {Name} {{");

                foreach (var attr in Attributes)
                    sb.AppendLine("  " + attr);

                foreach (var method in MethodsBySignature.Values)
                    sb.AppendLine("  " + method);

                sb.AppendLine("}");
                return new NormalizedInput(sb.ToString()).ToString();
            }
        }
    }
}
