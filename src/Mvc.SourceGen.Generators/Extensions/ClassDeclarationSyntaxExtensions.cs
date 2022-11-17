namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class ClassDeclarationSyntaxExtensions
{
    public static ClassDeclarationSyntax WithStaticConstructor(this ClassDeclarationSyntax classDeclarationSyntax, BlockSyntax block)
    {
        return classDeclarationSyntax.AddMembers(SyntaxFactory.ConstructorDeclaration(classDeclarationSyntax.Identifier)
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                            .WithBody(block));
    }
}
