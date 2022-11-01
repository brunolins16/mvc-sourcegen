namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis;
using System;
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
            EmitExtensionMethod();

            var content = new StringBuilder();

            content.AppendLine("using Microsoft.AspNetCore.Mvc.ApplicationParts;");
            content.AppendLine("using System.Diagnostics.CodeAnalysis;");
            content.AppendLine("using System.Reflection;");

            content.AppendLine("namespace Mvc.SourceGen;");


            content.AppendLine("internal class SourceGenApplicationPart : ApplicationPart, IApplicationPartTypeProvider");
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
                content.AppendLine($"    GetTypeInfo<{type.ToDisplayString()}>(),");
            }

            content.AppendLine("        };");
            content.AppendLine("    }");
            content.AppendLine("");
            content.AppendLine("    /// <inheritdoc />");
            content.AppendLine("    public IEnumerable<TypeInfo> Types => _types;");
            content.AppendLine("    /// <inheritdoc />");
            content.AppendLine("    public override string Name => \"Mvc.SourceGen\"!;");
            content.AppendLine("}");

            _context.AddSource("SourceGenApplicationPart.g.cs", content.ToString());
        }

        private void EmitExtensionMethod()
        {

            var content = new StringBuilder();

            content.AppendLine("            namespace Microsoft.Extensions.DependencyInjection;");
            content.AppendLine("");
            content.AppendLine("            using Mvc.SourceGen;");
            content.AppendLine("");
            content.AppendLine("            internal static class SourceGenMvcBuilderExtensions");
            content.AppendLine("            {");
            content.AppendLine("                public static IMvcBuilder AddSourceGenControllers(this IMvcBuilder builder)");
            content.AppendLine("                {");
            content.AppendLine("                    return builder.ConfigureApplicationPartManager(appPartManager");
            content.AppendLine("                        => appPartManager.ApplicationParts.Add(new SourceGenApplicationPart()));");
            content.AppendLine("                }");
            content.AppendLine("            }");

            _context.AddSource("SourceGenMvcBuilderExtensions.g.cs", content.ToString());
        }
    }
}