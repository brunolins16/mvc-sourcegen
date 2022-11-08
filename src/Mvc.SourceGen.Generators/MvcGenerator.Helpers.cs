namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Diagnostics;

public sealed partial class MvcGenerator
{

    internal static ClassDeclarationSyntax GetSemanticTargetForControllersType(GeneratorSyntaxContext context)
    {
        var detectedType = (ClassDeclarationSyntax)context.Node;
        INamedTypeSymbol typeSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(detectedType);
        Debug.Assert(typeSymbol != null);

        return typeSymbol == null ||  typeSymbol.IsAbstract ||  typeSymbol.IsStatic ||  typeSymbol.IsGenericType || typeSymbol.DeclaredAccessibility != Accessibility.Public
            ? null
            : detectedType;
    }

    internal static bool IsSyntaxTargetForPublicTypes(SyntaxNode node) => node is ClassDeclarationSyntax;
}