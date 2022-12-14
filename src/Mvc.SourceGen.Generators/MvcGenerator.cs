namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

[Generator(LanguageNames.CSharp)]
public sealed partial class MvcGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        IncrementalValueProvider<ImmutableArray<ClassDeclarationSyntax>> parameterDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(static (s, _) => MvcGenerator.IsSyntaxTargetForPublicTypes(s),
               static (ctx, _) => MvcGenerator.GetSemanticTargetForControllersType(ctx))
           .Where(static m => m is not null)
           .Collect();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

        var compilationAndClasses =
            context.CompilationProvider.Combine(parameterDeclarations);

        context.RegisterSourceOutput(compilationAndClasses, (spc, source) =>
            Execute(spc, source.Left, source.Right));
    }

    public static void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<ClassDeclarationSyntax> typeSyntaxes)
    {
        //if (!System.Diagnostics.Debugger.IsAttached)
        //{
        //    System.Diagnostics.Debugger.Launch();
        //}

        var parser = new Parser(compilation);
        var spec = parser.Parse(typeSyntaxes);

        var emitter = new Emitter(context, spec);
        emitter.Emit();
    }
}