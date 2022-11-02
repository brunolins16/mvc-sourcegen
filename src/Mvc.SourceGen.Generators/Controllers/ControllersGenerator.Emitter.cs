namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;

public sealed partial class ControllersGenerator
{
    internal class Emitter
    {
        private static readonly IdentifierNameSyntax DINamespace = SyntaxFactory.IdentifierName("Microsoft.Extensions.DependencyInjection");
        private static readonly IdentifierNameSyntax SourceGenNamespace = SyntaxFactory.IdentifierName("Mvc.SourceGen");
        private static readonly IdentifierNameSyntax BuilderVariable = SyntaxFactory.IdentifierName("builder");
        private static readonly IdentifierNameSyntax TypesVariable = SyntaxFactory.IdentifierName("_types");
        private static readonly IdentifierNameSyntax AppPartManagerParameter = SyntaxFactory.IdentifierName("appPartManager");
        private static readonly IdentifierNameSyntax MvcBuilderType = SyntaxFactory.IdentifierName("IMvcBuilder");
        private static readonly IdentifierNameSyntax SourceGenAppPartType = SyntaxFactory.IdentifierName("SourceGenApplicationPart");
        private static readonly IdentifierNameSyntax AppPartType = SyntaxFactory.IdentifierName("ApplicationPart");
        private static readonly IdentifierNameSyntax AppPartTypeProviderType = SyntaxFactory.IdentifierName("IApplicationPartTypeProvider");
        private static readonly IdentifierNameSyntax TypeInfoType = SyntaxFactory.IdentifierName("TypeInfo");
        private static readonly IdentifierNameSyntax EnumerableType = SyntaxFactory.IdentifierName("IEnumerable");
        private static readonly GenericNameSyntax TypeInfoIEnumrableType = SyntaxFactory.GenericName(EnumerableType.Identifier, SyntaxFactory.TypeArgumentList().AddArguments(TypeInfoType));
        private static readonly IdentifierNameSyntax DynamicallyAccessedMemberTypesType = SyntaxFactory.IdentifierName("DynamicallyAccessedMemberTypes");
        

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
            EmitSourceGenAppPart();

        }

        private void EmitSourceGenAppPart()
        {
            var expressions = new ExpressionSyntax[_spec.ControllerTypes.Length];

            for (int i = 0; i < _spec.ControllerTypes.Length; i++)
            {
                // GetTypeInfo<{type.ToDisplayString()}>()
                expressions[i] = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("GetTypeInfo"), 
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SeparatedList(
                                new TypeSyntax[] { SyntaxFactory.IdentifierName(_spec.ControllerTypes[i].ToDisplayString()) })
                    )));
            }

            // namespace Mvc.SourceGen
            var declaration = SyntaxFactory.NamespaceDeclaration(SourceGenNamespace)
                .WithUsings(SyntaxFactory.List(new UsingDirectiveSyntax[]
                {
                    // using Microsoft.AspNetCore.Mvc.ApplicationParts;
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Mvc.ApplicationParts")),
                    // using System.Diagnostics.CodeAnalysis; 
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Diagnostics.CodeAnalysis")),
                    // using System.Reflection;
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Reflection"))
                }))
                .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                {
                    // internal class SourceGenApplicationPart : ApplicationPart, IApplicationPartTypeProvider
                    SyntaxFactory.ClassDeclaration(SourceGenAppPartType.Identifier)
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword)))
                        .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(new BaseTypeSyntax[]
                        {
                            SyntaxFactory.SimpleBaseType(AppPartType),
                            SyntaxFactory.SimpleBaseType(AppPartTypeProviderType),
                        })))
                        .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                        {
                            // private readonly static TypeInfo[] _types;
                            SyntaxFactory.FieldDeclaration(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.ArrayType(TypeInfoType)
                                        .AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier()
                                        .AddSizes(SyntaxFactory.OmittedArraySizeExpression())))
                                    .AddVariables(SyntaxFactory.VariableDeclarator(TypesVariable.Identifier)
                                ))
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))),

                            // static SourceGenApplicationPart()
                            SyntaxFactory.ConstructorDeclaration(SourceGenAppPartType.Identifier)
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ExpressionStatement(
                                        // _types = new TypeInfo[{_spec.ControllerTypes.Length}]
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression, 
                                            TypesVariable,
                                            SyntaxFactory.ArrayCreationExpression(
                                                SyntaxFactory.ArrayType(TypeInfoType)
                                                     .AddRankSpecifiers(
                                                        SyntaxFactory.ArrayRankSpecifier()
                                                            .AddSizes(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(_spec.ControllerTypes.Length))))
                                                )
                                            .WithInitializer(
                                                SyntaxFactory.InitializerExpression(
                                                    SyntaxKind.ArrayInitializerExpression,
                                                    SyntaxFactory.SeparatedList(expressions)
                                            ))))
                                 )),

                            // public IEnumerable<TypeInfo> Types => _types;
                            SyntaxFactory.PropertyDeclaration(TypeInfoIEnumrableType, "Types")
                                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(TypesVariable))
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))),

                            // public override string Name => "Mvc.SourceGen"
                            SyntaxFactory.PropertyDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), "Name")
                                .WithExpressionBody(
                                    SyntaxFactory.ArrowExpressionClause(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("Mvc.SourceGen"))))
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword))),

                            // internal static TypeInfo GetTypeInfo<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
                            SyntaxFactory.MethodDeclaration(TypeInfoType, "GetTypeInfo")
                                .WithTypeParameterList(SyntaxFactory.TypeParameterList().AddParameters(
                                        SyntaxFactory.TypeParameter(SyntaxFactory.Identifier("T"))
                                            .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new AttributeSyntax[] 
                                            {
                                                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("DynamicallyAccessedMembers"))
                                                    .AddArgumentListArguments(
                                                        SyntaxFactory.AttributeArgument(
                                                            SyntaxFactory.BinaryExpression(
                                                                SyntaxKind.BitwiseOrExpression,
                                                                SyntaxFactory.BinaryExpression(
                                                                    SyntaxKind.BitwiseOrExpression,
                                                                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, DynamicallyAccessedMemberTypesType, SyntaxFactory.IdentifierName("PublicProperties")),
                                                                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, DynamicallyAccessedMemberTypesType, SyntaxFactory.IdentifierName("PublicMethods"))),
                                                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, DynamicallyAccessedMemberTypesType, SyntaxFactory.IdentifierName("PublicConstructors")))))
                                            })))
                                    ))
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                                //  => typeof(T).GetTypeInfo();
                                .WithExpressionBody(
                                    SyntaxFactory.ArrowExpressionClause(
                                       SyntaxFactory.InvocationExpression(
                                           SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName("T")), SyntaxFactory.IdentifierName("GetTypeInfo")))
                                       ))
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))                            
                        }))
                }));

            _context.AddSource(
                "SourceGenApplicationPart.g.cs",
                declaration
                .NormalizeWhitespace()
                .GetText(encoding: Encoding.UTF8));
        }

        /// <summary>
        /// Emit the IMvcBuilder AddSourceGenControllers extension method
        /// </summary>
        private void EmitExtensionMethod()
        {
            // namespace Microsoft.Extensions.DependencyInjection
            var declaration = SyntaxFactory.NamespaceDeclaration(DINamespace)
                .WithUsings(SyntaxFactory.List(new UsingDirectiveSyntax[]
                {
                    //  using Mvc.SourceGen;
                    SyntaxFactory.UsingDirective(SourceGenNamespace)
                }))
                .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                {
                    // internal static class SourceGenMvcBuilderExtensions
                    SyntaxFactory.ClassDeclaration("SourceGenMvcBuilderExtensions")
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                        .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[] 
                        {
                            //public static IMvcBuilder AddSourceGenControllers(this IMvcBuilder builder)
                            SyntaxFactory.MethodDeclaration(MvcBuilderType, "AddSourceGenControllers")
                                .AddParameterListParameters(
                                    SyntaxFactory.Parameter(BuilderVariable.Identifier)
                                        .WithType(MvcBuilderType)
                                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ThisKeyword)))
                                )
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                                .WithBody(SyntaxFactory.Block(
                                    //  return builder.ConfigureApplicationPartManager
                                    SyntaxFactory.ReturnStatement(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, 
                                                BuilderVariable, 
                                                SyntaxFactory.IdentifierName("ConfigureApplicationPartManager")
                                            ))
                                        .WithArgumentList(SyntaxFactory.ArgumentList().AddArguments(                                            
                                            SyntaxFactory.Argument(
                                                // appPartManager => 
                                                SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(AppPartManagerParameter.Identifier))
                                                    .WithExpressionBody(
                                                        // appPartManager.ApplicationParts.Add()
                                                        SyntaxFactory.InvocationExpression(
                                                             SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    AppPartManagerParameter, 
                                                                    SyntaxFactory.IdentifierName("ApplicationParts")),
                                                                SyntaxFactory.IdentifierName("Add")                                                                                                                        
                                                        ))
                                                        .WithArgumentList(SyntaxFactory.ArgumentList().AddArguments(
                                                               // new SourceGenApplicationPart()
                                                               SyntaxFactory.Argument(SyntaxFactory.ObjectCreationExpression(SourceGenAppPartType).WithArgumentList(SyntaxFactory.ArgumentList()))
                                                        ))
                                                    )
                                            )
                                         ))
                                )))
                        }))
                }));

            _context.AddSource(
                "SourceGenMvcBuilderExtensions.g.cs",
                declaration
                .NormalizeWhitespace()
                .GetText(encoding: Encoding.UTF8));
        }
    }
}