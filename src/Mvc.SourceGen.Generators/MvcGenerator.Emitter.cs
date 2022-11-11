namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using System.Collections.Generic;
using System;
using Roslyn.Reflection;
using System.Reflection;

public sealed partial class MvcGenerator
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
            EmitControllerTypeProvider();
            EmitModelMetadataProvider();
        }

        /// <summary>
        /// 
        /// </summary>
        private void EmitModelMetadataProvider()
        {
            // Includes all detected types + MvcSourceGenContext
            var classDeclarations = new MemberDeclarationSyntax[_spec.ModelTypes.Length];
            var typeMapping = new Dictionary<string, IdentifierNameSyntax>(StringComparer.OrdinalIgnoreCase);

            // TODO: Check if it works with generics + records + nullables
            for (int i = 0; i < _spec.ModelTypes.Length; i++)
            {
                var modelType = _spec.ModelTypes[i];
                const string suffix = "ModelMetadata";

                IdentifierNameSyntax metadataType;

                if (modelType.SpecialType != SpecialType.None)
                {
                    metadataType = SyntaxFactory.IdentifierName($"{modelType.SpecialType}{suffix}");
                }
                else
                {
                    // TODO: HACK 😅 Fix + performance
                    metadataType = SyntaxFactory.IdentifierName(
                                        $"{modelType.ToDisplayString()}{suffix}"
                                        .Replace(".", "")
                                        .Replace("?", "Nullable")
                                        .Replace(">", "")
                                        .Replace("<", ""));

                }

                typeMapping[modelType.ToDisplayString()] = metadataType;
                classDeclarations[i] = SyntaxFactory.ClassDeclaration(metadataType.Identifier)
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.FileKeyword)))
                        .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(new BaseTypeSyntax[]
                        {
                            SyntaxFactory.SimpleBaseType(SyntaxFactory.GenericName(TypeSyntaxConstants.ModelMetadataType.Identifier, SyntaxFactory.TypeArgumentList().AddArguments(SyntaxFactory.IdentifierName(modelType.ToDisplayString()))))
                        })))
                        .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                        {
                            //public {name}(
                            //  ISourceGenModelMetadataProvider applicationModelMetadataProvider,
                            //  IModelMetadataProvider provider,
                            //  ICompositeMetadataDetailsProvider detailsProvider,
                            //  DefaultMetadataDetails details,
                            //  DefaultModelBindingMessageProvider modelBindingMessageProvider)
                            SyntaxFactory.ConstructorDeclaration(metadataType.Identifier)
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                .AddParameterListParameters(
                                    SyntaxFactory.Parameter(TypeSyntaxConstants.ApplicationModelMetadataProviderVariable.Identifier).WithType(TypeSyntaxConstants.ModelMetadataProviderType),
                                    SyntaxFactory.Parameter(TypeSyntaxConstants.ProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("IModelMetadataProvider")),
                                    SyntaxFactory.Parameter(TypeSyntaxConstants.DetailsProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("ICompositeMetadataDetailsProvider")),
                                    SyntaxFactory.Parameter(TypeSyntaxConstants.DetailsVariable.Identifier).WithType(SyntaxFactory.IdentifierName("DefaultMetadataDetails")),
                                    SyntaxFactory.Parameter(TypeSyntaxConstants.ModelBindingMessageProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("DefaultModelBindingMessageProvider"))
                                )
                                // : base(applicationModelMetadataProvider, provider, detailsProvider, details, modelBindingMessageProvider)
                                .WithInitializer(SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer).AddArgumentListArguments(
                                    SyntaxFactory.Argument(TypeSyntaxConstants.ApplicationModelMetadataProviderVariable),
                                    SyntaxFactory.Argument(TypeSyntaxConstants.ProviderVariable),
                                    SyntaxFactory.Argument(TypeSyntaxConstants.DetailsProviderVariable),
                                    SyntaxFactory.Argument(TypeSyntaxConstants.DetailsVariable),
                                    SyntaxFactory.Argument(TypeSyntaxConstants.ModelBindingMessageProviderVariable)
                                    ))
                                // {}
                                .WithBody(SyntaxFactory.Block()),
                        }))
                        .WithPropertyInitMethod(modelType)
                        .WithCtorInitMethod(modelType)
                        .WithLeadingTrivia(SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, $"// {_spec.ModelTypes[i].MetadataName}"));
            }

            // namespace Mvc.SourceGen ?? App namespace
            var declaration = SyntaxFactory.NamespaceDeclaration(TypeSyntaxConstants.SourceGenNamespace)
                .WithUsings(SyntaxFactory.List(new UsingDirectiveSyntax[]
                {
                    // using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Mvc.ModelBinding.Metadata")),
                    // using Microsoft.AspNetCore.Mvc.ModelBinding;
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Mvc.ModelBinding")),
                    // using System.Diagnostics.CodeAnalysis; 
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Diagnostics.CodeAnalysis"))
                }))
                .WithLeadingTrivia(SyntaxFactory.Trivia(SyntaxFactory.NullableDirectiveTrivia(SyntaxFactory.Token(SyntaxKind.EnableKeyword), true)))
                .AddMembers(EmitContext(typeMapping))
                .AddMembers(classDeclarations);

            _context.AddSource(
                "MvcSourceGenContext.ModelMetadataProvider.g.cs",
                declaration
                .NormalizeWhitespace()
                .GetText(encoding: Encoding.UTF8));
        }


        private static ClassDeclarationSyntax EmitContext(Dictionary<string, IdentifierNameSyntax> typeMapping)
        {
            var expressions = new List<StatementSyntax>(typeMapping.Count + 2)
            {
                // modelMetadata = null;
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        TypeSyntaxConstants.ModelMetadataVariable,
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)))
            };
            foreach (var item in typeMapping)
            {
                expressions.Add(
                    SyntaxFactory.IfStatement(
                        SyntaxFactory.BinaryExpression(
                            SyntaxKind.EqualsExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    TypeSyntaxConstants.EntryVariable,
                                    SyntaxFactory.IdentifierName("Key")
                                    ),
                                SyntaxFactory.IdentifierName("ModelType")
                                ),
                            SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(item.Key))),
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                TypeSyntaxConstants.ModelMetadataVariable,
                                SyntaxFactory.ObjectCreationExpression(item.Value)
                                .AddArgumentListArguments(
                                    SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                                    SyntaxFactory.Argument(TypeSyntaxConstants.ProviderVariable),
                                    SyntaxFactory.Argument(TypeSyntaxConstants.DetailsProviderVariable),
                                    SyntaxFactory.Argument(TypeSyntaxConstants.EntryVariable),
                                    SyntaxFactory.Argument(TypeSyntaxConstants.ModelBindingMessageProviderVariable)
                                    )
                                ))
                        ));
            }

            // return modelMetadata != null
            expressions.Add(SyntaxFactory.ReturnStatement(
                SyntaxFactory.BinaryExpression(
                    SyntaxKind.NotEqualsExpression,
                    TypeSyntaxConstants.ModelMetadataVariable,
                    SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))));

            // internal partial class MvcSourceGenContext : ISourceGenModelMetadataProvider
            return SyntaxFactory.ClassDeclaration(TypeSyntaxConstants.MvcSourceGenContextType.Identifier)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(new BaseTypeSyntax[]
                {
                    SyntaxFactory.SimpleBaseType(TypeSyntaxConstants.ModelMetadataProviderType),
                })))
                .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                {
                    // bool TryCreateModelMetadata(, , , , );
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), "TryCreateModelMetadata")
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                        .AddParameterListParameters(
                            // DefaultMetadataDetails entry
                            SyntaxFactory.Parameter(TypeSyntaxConstants.EntryVariable.Identifier).WithType(SyntaxFactory.IdentifierName("DefaultMetadataDetails")),
                            // IModelMetadataProvider provider
                            SyntaxFactory.Parameter(TypeSyntaxConstants.ProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("IModelMetadataProvider")),
                            // ICompositeMetadataDetailsProvider detailsProvider
                            SyntaxFactory.Parameter(TypeSyntaxConstants.DetailsProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("ICompositeMetadataDetailsProvider")),
                            // DefaultModelBindingMessageProvider modelBindingMessageProvider
                            SyntaxFactory.Parameter(TypeSyntaxConstants.ModelBindingMessageProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("DefaultModelBindingMessageProvider")),
                            //[NotNullWhen(true)] out ModelMetadata? modelMetadata
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("modelMetadata"))
                                .WithType(SyntaxFactory.IdentifierName("ModelMetadata?"))
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.OutKeyword)))
                        )
                        //  => null;
                        .WithBody(SyntaxFactory.Block(expressions))
                }));
        }

        /// <summary>
        /// 
        /// </summary>
        private void EmitControllerTypeProvider()
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

            // namespace Mvc.SourceGen ?? App namespace
            var declaration = SyntaxFactory.NamespaceDeclaration(TypeSyntaxConstants.SourceGenNamespace)
                .WithUsings(SyntaxFactory.List(new UsingDirectiveSyntax[]
                {
                    // using System.Diagnostics.CodeAnalysis; 
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Diagnostics.CodeAnalysis")),
                    // using System.Reflection;
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Reflection"))
                }))
                .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                {
                    // internal partial class MvcSourceGenContext : ISourceGenControllerTypeProvider
                    SyntaxFactory.ClassDeclaration(TypeSyntaxConstants.MvcSourceGenContextType.Identifier)
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                        .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(new BaseTypeSyntax[]
                        {
                            SyntaxFactory.SimpleBaseType(TypeSyntaxConstants.ControllerTypeProviderType),
                        })))
                        .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                        {
                            // private readonly static TypeInfo[] _types;
                            SyntaxFactory.FieldDeclaration(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.ArrayType(TypeSyntaxConstants.TypeInfoType)
                                        .AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier()
                                        .AddSizes(SyntaxFactory.OmittedArraySizeExpression())))
                                    .AddVariables(SyntaxFactory.VariableDeclarator(TypeSyntaxConstants.TypesVariable.Identifier)
                                ))
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))),

                            // static MvcSourceGenContext()
                            SyntaxFactory.ConstructorDeclaration(TypeSyntaxConstants.MvcSourceGenContextType.Identifier)
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ExpressionStatement(
                                        // _types = new TypeInfo[{_spec.ControllerTypes.Length}]
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            TypeSyntaxConstants.TypesVariable,
                                            SyntaxFactory.ArrayCreationExpression(
                                                SyntaxFactory.ArrayType(TypeSyntaxConstants.TypeInfoType)
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

                            // public IEnumerable<TypeInfo> ControllerTypes => _types;
                            SyntaxFactory.PropertyDeclaration(TypeSyntaxConstants.TypeInfoIEnumrableType, "ControllerTypes")
                                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(TypeSyntaxConstants.TypesVariable))
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))),

                            // internal static TypeInfo GetTypeInfo<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
                            SyntaxFactory.MethodDeclaration(TypeSyntaxConstants.TypeInfoType, "GetTypeInfo")
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
                                                                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, TypeSyntaxConstants.DynamicallyAccessedMemberTypesType, SyntaxFactory.IdentifierName("PublicProperties")),
                                                                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, TypeSyntaxConstants.DynamicallyAccessedMemberTypesType, SyntaxFactory.IdentifierName("PublicMethods"))),
                                                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, TypeSyntaxConstants.DynamicallyAccessedMemberTypesType, SyntaxFactory.IdentifierName("PublicConstructors")))))
                                            })))
                                    ))
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
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
                "MvcSourceGenContext.ControllerTypeProvider.g.cs",
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
            var declaration = SyntaxFactory.NamespaceDeclaration(TypeSyntaxConstants.DINamespace)
                .WithUsings(SyntaxFactory.List(new UsingDirectiveSyntax[]
                {
                    //  using Mvc.SourceGen;
                    SyntaxFactory.UsingDirective(TypeSyntaxConstants.SourceGenNamespace)
                }))
                .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                {
                    // internal static class SourceGenMvcBuilderExtensions
                    SyntaxFactory.ClassDeclaration("SourceGenMvcBuilderExtensions")
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                        .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                        {
                            //public static IMvcBuilder AddSourceGeneratorProviders(this IMvcBuilder builder)
                            SyntaxFactory.MethodDeclaration(TypeSyntaxConstants.MvcBuilderType, "AddSourceGeneratorProviders")
                                .AddParameterListParameters(
                                    SyntaxFactory.Parameter(TypeSyntaxConstants.BuilderVariable.Identifier)
                                        .WithType(TypeSyntaxConstants.MvcBuilderType)
                                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ThisKeyword)))
                                )
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                                .WithBody(SyntaxFactory.Block(
                                    //var mvcSourceGenContext = new MvcSourceGenContext();
                                    SyntaxFactory.LocalDeclarationStatement(
                                        SyntaxFactory.VariableDeclaration(TypeSyntaxConstants.MvcSourceGenContextType)
                                            .AddVariables(
                                                SyntaxFactory.VariableDeclarator(TypeSyntaxConstants.ContextVariable.Identifier)
                                                    .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ImplicitObjectCreationExpression()))
                                            )
                                     ),
                                    
                                    //  return builder.AddSourceGeneratorProviders
                                    SyntaxFactory.ReturnStatement(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                TypeSyntaxConstants.BuilderVariable,
                                                SyntaxFactory.IdentifierName("AddSourceGeneratorProviders")
                                            ))
                                        .WithArgumentList(SyntaxFactory.ArgumentList().AddArguments(
                                            // ISourceGenControllerTypeProvider
                                            SyntaxFactory.Argument(TypeSyntaxConstants.ContextVariable),
                                            // ISourceGenModelMetadataProvider
                                            SyntaxFactory.Argument(TypeSyntaxConstants.ContextVariable)
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