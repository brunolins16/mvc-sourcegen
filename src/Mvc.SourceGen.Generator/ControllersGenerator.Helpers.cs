namespace Mvc.SourceGen.Generator;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

public sealed partial class ControllersGenerator
{

    internal static ClassDeclarationSyntax GetSemanticTargetForControllersType(GeneratorSyntaxContext context)
    {
        var detectedType = (ClassDeclarationSyntax)context.Node;
        INamedTypeSymbol typeSymbol = (INamedTypeSymbol)(context.SemanticModel.GetDeclaredSymbol(detectedType));
        Debug.Assert(typeSymbol != null);

        if (typeSymbol == null ||  typeSymbol.IsAbstract ||  typeSymbol.IsStatic ||  typeSymbol.IsGenericType || typeSymbol.DeclaredAccessibility != Accessibility.Public)
        {
            return null;
        }


        return detectedType;
    }

    internal static bool IsSyntaxTargetForPublicTypes(SyntaxNode node) => node is ClassDeclarationSyntax;
}