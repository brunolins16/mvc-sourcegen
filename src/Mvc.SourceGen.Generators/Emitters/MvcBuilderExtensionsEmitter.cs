namespace Mvc.SourceGen.Generators.Emitters;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

/// <summary>
/// Emit the IMvcBuilder AddSourceGenControllers extension method
/// </summary>
internal class MvcBuilderExtensionsEmitter : IEmitter
{
    public static readonly IdentifierNameSyntax DINamespace = SyntaxFactory.IdentifierName("Microsoft.Extensions.DependencyInjection");

    public static readonly IdentifierNameSyntax MvcSourceGenContextQualified = SyntaxFactory.IdentifierName("Mvc.SourceGen.MvcSourceGenContext");
    public static readonly IdentifierNameSyntax MvcBuilderType = SyntaxFactory.IdentifierName("IMvcBuilder");
    public static readonly IdentifierNameSyntax BuilderVariable = SyntaxFactory.IdentifierName("builder");
    public static readonly IdentifierNameSyntax ContextVariable = SyntaxFactory.IdentifierName("mvcSourceGenContext");

    public void Emit(SourceProductionContext context, SourceGenerationSpec spec)
    {
        // namespace Microsoft.Extensions.DependencyInjection
        var declaration = SyntaxFactory.NamespaceDeclaration(DINamespace)
            .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
            {
                    // internal static class SourceGenMvcBuilderExtensions
                    SyntaxFactory.ClassDeclaration("SourceGenMvcBuilderExtensions")
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                        .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                        {
                            //public static IMvcBuilder AddMvcContext(this IMvcBuilder builder)
                            SyntaxFactory.MethodDeclaration(MvcBuilderType, "AddMvcContext")
                                .AddParameterListParameters(
                                    SyntaxFactory.Parameter(BuilderVariable.Identifier)
                                        .WithType(MvcBuilderType)
                                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ThisKeyword)))
                                )
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                                .WithBody(SyntaxFactory.Block(
                                    //var mvcSourceGenContext = new MvcSourceGenContext();
                                    SyntaxFactory.LocalDeclarationStatement(
                                        SyntaxFactory.VariableDeclaration(MvcSourceGenContextQualified)
                                            .AddVariables(
                                                SyntaxFactory.VariableDeclarator(ContextVariable.Identifier)
                                                    .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ImplicitObjectCreationExpression()))
                                            )
                                     ),
                                    
                                    //  return builder.AddMvcContext
                                    SyntaxFactory.ReturnStatement(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                BuilderVariable,
                                                SyntaxFactory.IdentifierName("AddMvcContext")
                                            ))
                                        .WithArgumentList(SyntaxFactory.ArgumentList().AddArguments(
                                            // ISourceGenContext
                                            SyntaxFactory.Argument(ContextVariable)))
                                )))
                        }))
            }));

        context.AddSource(
            "MvcSourceGenContext.IMvcBuilderExtensions.g.cs",
            declaration
            .NormalizeWhitespace()
            .GetText(encoding: Encoding.UTF8));
    }
}
