namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

[Generator(LanguageNames.CSharp)]
public sealed partial class ControllersGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var parameterDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(static (s, _) => ControllersGenerator.IsSyntaxTargetForPublicTypes(s),
               static (ctx, _) => ControllersGenerator.GetSemanticTargetForControllersType(ctx))
           .Where(static m => m is not null)
           .Collect();

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

        var parser = new Parser(context, compilation);
        var spec = parser.Parse(typeSyntaxes);

        var emitter = new Emitter(context, spec);
        emitter.Emit();
    }
}