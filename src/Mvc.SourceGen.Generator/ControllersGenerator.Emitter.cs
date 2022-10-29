namespace Mvc.SourceGen.Generator;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

public sealed partial class ControllersGenerator
{
    internal class Emitter
    {
        private readonly SourceProductionContext _context;
        private readonly SourceGenerationSpec _spec;

        public Emitter(SourceProductionContext context, SourceGenerationSpec spec)
        {
            _context = context;
            _spec = spec;
        }

        public void Emit()
        {
            var content = new StringBuilder();

            content.AppendLine("using Microsoft.AspNetCore.Mvc.ApplicationParts;");
            content.AppendLine("using Mvc.SourceGen.Web.Controllers;");
            content.AppendLine("using System.Diagnostics.CodeAnalysis;");
            content.AppendLine("using System.Reflection;");

            content.AppendLine("namespace Mvc.SourceGen.Web;");

           
            content.AppendLine("public partial class SourceGenApplicationPart : IApplicationPartTypeProvider");
            content.AppendLine("{");
            content.AppendLine("    private readonly static TypeInfo[] _types;");
            content.AppendLine("");
            content.AppendLine("    internal static TypeInfo GetTypeInfo<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)] T>()");
            content.AppendLine("        => typeof(T).GetTypeInfo();");
            content.AppendLine("");
            content.AppendLine("    static SourceGenApplicationPart()");
            content.AppendLine("    {");
            content.AppendLine($"        _types = new TypeInfo[{_spec.ControllerTypes.Length}]");
            content.AppendLine("        {");

            foreach (var type in _spec.ControllerTypes)
            {
                content.AppendLine($"    GetTypeInfo<{type.MetadataName}>(),");
            }
            
            content.AppendLine("        };");
            content.AppendLine("    }");
            content.AppendLine("");
            content.AppendLine("    /// <inheritdoc />");
            content.AppendLine("    public IEnumerable<TypeInfo> Types => _types;");
            content.AppendLine("}");

            _context.AddSource("SourceGenApplicationPart.g.cs", content.ToString());
        }
    }
}