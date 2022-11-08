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
        // Namespaces
        private static readonly IdentifierNameSyntax DINamespace = SyntaxFactory.IdentifierName("Microsoft.Extensions.DependencyInjection");
        private static readonly IdentifierNameSyntax SourceGenNamespace = SyntaxFactory.IdentifierName("Mvc.SourceGen");

        // Types
        private static readonly IdentifierNameSyntax VarType = SyntaxFactory.IdentifierName("var");
        private static readonly IdentifierNameSyntax MvcBuilderType = SyntaxFactory.IdentifierName("IMvcBuilder");
        private static readonly IdentifierNameSyntax MvcSourceGenContextType = SyntaxFactory.IdentifierName("MvcSourceGenContext");
        private static readonly IdentifierNameSyntax ControllerTypeProviderType = SyntaxFactory.IdentifierName("ISourceGenControllerTypeProvider");
        private static readonly IdentifierNameSyntax ModelMetadataProviderType = SyntaxFactory.IdentifierName("ISourceGenModelMetadataProvider");
        private static readonly IdentifierNameSyntax TypeInfoType = SyntaxFactory.IdentifierName("TypeInfo");
        private static readonly IdentifierNameSyntax EnumerableType = SyntaxFactory.IdentifierName("IEnumerable");
        private static readonly GenericNameSyntax TypeInfoIEnumrableType = SyntaxFactory.GenericName(EnumerableType.Identifier, SyntaxFactory.TypeArgumentList().AddArguments(TypeInfoType));
        private static readonly IdentifierNameSyntax DynamicallyAccessedMemberTypesType = SyntaxFactory.IdentifierName("DynamicallyAccessedMemberTypes");
        private static readonly IdentifierNameSyntax ModelMetadataType = SyntaxFactory.IdentifierName("ModelMetadata");
        private static readonly ArrayTypeSyntax ModelMetadataArrayType = SyntaxFactory.ArrayType(ModelMetadataType)
                                                                                           .AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier()
                                                                                            .AddSizes(SyntaxFactory.OmittedArraySizeExpression()));

        // variables
        private static readonly IdentifierNameSyntax BuilderVariable = SyntaxFactory.IdentifierName("builder");
        private static readonly IdentifierNameSyntax TypesVariable = SyntaxFactory.IdentifierName("_types");
        private static readonly IdentifierNameSyntax ContextVariable = SyntaxFactory.IdentifierName("mvcSourceGenContext");
        private static readonly IdentifierNameSyntax ProviderVariable = SyntaxFactory.IdentifierName("provider");
        private static readonly IdentifierNameSyntax DetailsProviderVariable = SyntaxFactory.IdentifierName("detailsProvider");
        private static readonly IdentifierNameSyntax DetailsVariable = SyntaxFactory.IdentifierName("details");
        private static readonly IdentifierNameSyntax ModelBindingMessageProviderVariable = SyntaxFactory.IdentifierName("modelBindingMessageProvider");
        private static readonly IdentifierNameSyntax ApplicationModelMetadataProviderVariable = SyntaxFactory.IdentifierName("applicationModelMetadataProvider");
        private static readonly IdentifierNameSyntax EntryVariable = SyntaxFactory.IdentifierName("entry");
        private static readonly IdentifierNameSyntax ModelMetadataVariable = SyntaxFactory.IdentifierName("modelMetadata");


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
            var classDeclarations = new MemberDeclarationSyntax[_spec.ModelTypes.Length + 1];
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
                    // TODO: Fix + performance
                    metadataType = SyntaxFactory.IdentifierName(
                                        $"{modelType.ToDisplayString()}{suffix}"
                                        .Replace(".", "")
                                        .Replace("?", "Nullable")
                                        .Replace(">", "")
                                        .Replace("<", ""));

                }

                typeMapping[modelType.ToDisplayString()] = metadataType;
                classDeclarations[i + 1] = SyntaxFactory.ClassDeclaration(metadataType.Identifier)
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.FileKeyword)))
                        .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(new BaseTypeSyntax[]
                        {
                            SyntaxFactory.SimpleBaseType(SyntaxFactory.GenericName(ModelMetadataType.Identifier, SyntaxFactory.TypeArgumentList().AddArguments(SyntaxFactory.IdentifierName(modelType.ToDisplayString()))))
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
                                    SyntaxFactory.Parameter(ApplicationModelMetadataProviderVariable.Identifier).WithType(ModelMetadataProviderType),
                                    SyntaxFactory.Parameter(ProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("IModelMetadataProvider")),
                                    SyntaxFactory.Parameter(DetailsProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("ICompositeMetadataDetailsProvider")),
                                    SyntaxFactory.Parameter(DetailsVariable.Identifier).WithType(SyntaxFactory.IdentifierName("DefaultMetadataDetails")),
                                    SyntaxFactory.Parameter(ModelBindingMessageProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("DefaultModelBindingMessageProvider"))
                                )
                                // : base(applicationModelMetadataProvider, provider, detailsProvider, details, modelBindingMessageProvider)
                                .WithInitializer(SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer).AddArgumentListArguments(
                                    SyntaxFactory.Argument(ApplicationModelMetadataProviderVariable),
                                    SyntaxFactory.Argument(ProviderVariable),
                                    SyntaxFactory.Argument(DetailsProviderVariable),
                                    SyntaxFactory.Argument(DetailsVariable),
                                    SyntaxFactory.Argument(ModelBindingMessageProviderVariable)
                                    ))
                                // {}
                                .WithBody(SyntaxFactory.Block()),

                            //protected override ModelMetadata[] PropertiesInit()
                            SyntaxFactory.MethodDeclaration(ModelMetadataArrayType, "PropertiesInit")
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword),SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
                                .WithBody(CreatePropInitMethod(modelType))
                        }))
                        .WithLeadingTrivia(SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, $"// {_spec.ModelTypes[i].MetadataName}"));
            }

            classDeclarations[0] = EmitContext(typeMapping);

            // namespace Mvc.SourceGen ?? App namespace
            var declaration = SyntaxFactory.NamespaceDeclaration(SourceGenNamespace)
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
                .WithMembers(SyntaxFactory.List(classDeclarations));

            _context.AddSource(
                "MvcSourceGenContext.ModelMetadataProvider.g.cs",
                declaration
                .NormalizeWhitespace()
                .GetText(encoding: Encoding.UTF8));
        }

        private BlockSyntax CreatePropInitMethod(INamedTypeSymbol modelType)
        {
            //TODO: Nullable?

            var isValueType = modelType.IsValueType;

            var properties = GetProperties(modelType);

            if(properties.Length == 0 || modelType.SpecialType != SpecialType.None)
            {
                // Array.Empty<ModelMetadata>()
                return SyntaxFactory.Block(SyntaxFactory.ReturnStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("Array"), SyntaxFactory.IdentifierName("Empty<ModelMetadata>")))));
            }

            var methodBlock = SyntaxFactory.Block();     
            var metadataInitializers = new ExpressionSyntax[properties.Length];

            for (int i = 0; i < properties.Length; i++)
            {
                var propertyType = properties[i].Type;

                if (propertyType.NullableAnnotation == NullableAnnotation.Annotated && propertyType.IsReferenceType)
                {
                    propertyType = propertyType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
                }

                // static (obj)
                var getter = SyntaxFactory.ParenthesizedLambdaExpression()
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                    .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("obj")))
                    .WithExpressionBody(
                        properties[i].IsWriteOnly ?
                            // => throw new InvalidOperationException
                            SyntaxFactory.ThrowExpression(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("InvalidOperationException"))
                                .AddArgumentListArguments(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("Writeonly property"))))) :
                            // => (({type})obj).Text!,
                            SyntaxFactory.PostfixUnaryExpression(
                                SyntaxKind.SuppressNullableWarningExpression, 
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression, 
                                    SyntaxFactory.ParenthesizedExpression(SyntaxFactory.CastExpression(SyntaxFactory.IdentifierName(modelType.ToDisplayString()), SyntaxFactory.IdentifierName("obj"))), 
                                    SyntaxFactory.IdentifierName(properties[i].Name))
                            )
                    );

                // static (obj, value)
                var setter = SyntaxFactory.ParenthesizedLambdaExpression()
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("obj")), 
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("value")))
                    .WithExpressionBody(
                        properties[i].IsReadOnly || properties[i].SetMethod.IsInitOnly ?
                             // => throw new InvalidOperationException
                             SyntaxFactory.ThrowExpression(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("InvalidOperationException"))
                                .AddArgumentListArguments(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("Readonly property"))))) :
                        isValueType ?
                            // => static (obj, value) => System.Runtime.CompilerServices.Unsafe.Unbox<global::Mvc.SourceGen.Web.Controllers.Message>(obj).Name = (string?)value,
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName($"System.Runtime.CompilerServices.Unsafe.Unbox<{modelType.ToDisplayString()}>"))
                                        .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("obj"))),
                                    SyntaxFactory.IdentifierName(properties[i].Name)),
                                SyntaxFactory.CastExpression(
                                    SyntaxFactory.IdentifierName(properties[i].Type.ToDisplayString()), 
                                    SyntaxFactory.PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression, SyntaxFactory.IdentifierName("value")))
                            ) :
                            // static (obj, value) => ((MessageRef)obj).Text = (string)value!
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.ParenthesizedExpression(SyntaxFactory.CastExpression(SyntaxFactory.IdentifierName(modelType.ToDisplayString()), SyntaxFactory.IdentifierName("obj"))),
                                    SyntaxFactory.IdentifierName(properties[i].Name)),
                                SyntaxFactory.CastExpression(
                                    SyntaxFactory.IdentifierName(properties[i].Type.ToDisplayString()), 
                                    SyntaxFactory.PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression, SyntaxFactory.IdentifierName("value")))
                            )
                    );
          

                methodBlock = methodBlock.AddStatements(

                        //var propertyInfo{i} = declaredType.GetProperty("");
                        SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(VarType).AddVariables(
                            SyntaxFactory.VariableDeclarator($"propertyInfo{i}")
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(modelType.ToDisplayString())),
                                            SyntaxFactory.IdentifierName("GetProperty"))
                                        )
                                    .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(properties[i].Name))))
                            )))),

                        //var propertyInfo0_key = ModelMetadataIdentity.ForProperty(propertyInfo0!, typeof(string), declaredType);
                        SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(VarType).AddVariables(
                            SyntaxFactory.VariableDeclarator($"propertyInfo{i}_key")
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.IdentifierName("ModelMetadataIdentity"),
                                            SyntaxFactory.IdentifierName("ForProperty"))
                                        )
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName($"propertyInfo{i}")),
                                        SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(propertyType.ToDisplayString()))),
                                        SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(modelType.ToDisplayString()))))
                            )))),

                        //var propertyInfo0_attributes = ModelAttributes.GetAttributesForProperty(declaredType, propertyInfo0!, typeof(string));
                        SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(VarType).AddVariables(
                            SyntaxFactory.VariableDeclarator($"propertyInfo{i}_attributes")
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.IdentifierName("ModelAttributes"),
                                            SyntaxFactory.IdentifierName("GetAttributesForProperty"))
                                        )
                                    .AddArgumentListArguments(
                                        SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(modelType.ToDisplayString()))),
                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName($"propertyInfo{i}")),
                                        SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(propertyType.ToDisplayString()))))
                            )))),

                        // var propertyInfo0_entry = new DefaultMetadataDetails(propertyInfo{i}_key, propertyInfo0_attributes)
                        SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(VarType).AddVariables(
                            SyntaxFactory.VariableDeclarator($"propertyInfo{i}_entry")
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("DefaultMetadataDetails"))
                                        .AddArgumentListArguments(
                                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName($"propertyInfo{i}_key")),
                                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName($"propertyInfo{i}_attributes"))
                                        )
                                        .WithInitializer(
                                            SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression)
                                            .AddExpressions(
                                                //  PropertyGetter = 
                                                SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName("PropertyGetter"), getter),
                                                //  PropertySetter = 
                                                SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName("PropertySetter"), setter))
                                        )
                            ))))
                );

                //  CreateMetadata(propertyInfo0_entry)
                metadataInitializers[i] = SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("CreateMetadata"))
                    .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName($"propertyInfo{i}_entry")));
            }

            return methodBlock.AddStatements(
                SyntaxFactory.ReturnStatement(SyntaxFactory.ImplicitArrayCreationExpression(SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression).AddExpressions(metadataInitializers))));
        }

        private IPropertySymbol[] GetProperties(INamedTypeSymbol modelType)
        {
            List<IPropertySymbol> properties = default;
            foreach (var t in modelType.BaseTypes())
            {
                foreach (var symbol in t.GetMembers().OfType<IPropertySymbol>())
                {
                    if (symbol.DeclaredAccessibility == Accessibility.Public)
                    {
                        properties ??= new();
                        properties.Add(symbol);
                    }
                }
            }

            return properties?.ToArray() ?? Array.Empty<IPropertySymbol>();
        }

        private static ClassDeclarationSyntax EmitContext(Dictionary<string, IdentifierNameSyntax> typeMapping)
        {
            var expressions = new List<StatementSyntax>(typeMapping.Count + 2)
            {
                // modelMetadata = null;
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        ModelMetadataVariable,
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
                                    EntryVariable,
                                    SyntaxFactory.IdentifierName("Key")
                                    ),
                                SyntaxFactory.IdentifierName("ModelType")
                                ),
                            SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(item.Key))),
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                ModelMetadataVariable,
                                SyntaxFactory.ObjectCreationExpression(item.Value)
                                .AddArgumentListArguments(
                                    SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                                    SyntaxFactory.Argument(ProviderVariable),
                                    SyntaxFactory.Argument(DetailsProviderVariable),
                                    SyntaxFactory.Argument(EntryVariable),
                                    SyntaxFactory.Argument(ModelBindingMessageProviderVariable)
                                    )
                                ))
                        ));
            }

            // return modelMetadata != null
            expressions.Add(SyntaxFactory.ReturnStatement(
                SyntaxFactory.BinaryExpression(
                    SyntaxKind.NotEqualsExpression,
                    ModelMetadataVariable,
                    SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))));

            // internal partial class MvcSourceGenContext : ISourceGenModelMetadataProvider
            return SyntaxFactory.ClassDeclaration(MvcSourceGenContextType.Identifier)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(new BaseTypeSyntax[]
                {
                    SyntaxFactory.SimpleBaseType(ModelMetadataProviderType),
                })))
                .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                {
                    // bool TryCreateModelMetadata(, , , , );
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), "TryCreateModelMetadata")
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                        .AddParameterListParameters(
                            // DefaultMetadataDetails entry
                            SyntaxFactory.Parameter(EntryVariable.Identifier).WithType(SyntaxFactory.IdentifierName("DefaultMetadataDetails")),
                            // IModelMetadataProvider provider
                            SyntaxFactory.Parameter(ProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("IModelMetadataProvider")),
                            // ICompositeMetadataDetailsProvider detailsProvider
                            SyntaxFactory.Parameter(DetailsProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("ICompositeMetadataDetailsProvider")),
                            // DefaultModelBindingMessageProvider modelBindingMessageProvider
                            SyntaxFactory.Parameter(ModelBindingMessageProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("DefaultModelBindingMessageProvider")),
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
            var declaration = SyntaxFactory.NamespaceDeclaration(SourceGenNamespace)
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
                    SyntaxFactory.ClassDeclaration(MvcSourceGenContextType.Identifier)
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                        .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(new BaseTypeSyntax[]
                        {
                            SyntaxFactory.SimpleBaseType(ControllerTypeProviderType),
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

                            // static MvcSourceGenContext()
                            SyntaxFactory.ConstructorDeclaration(MvcSourceGenContextType.Identifier)
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

                            // public IEnumerable<TypeInfo> ControllerTypes => _types;
                            SyntaxFactory.PropertyDeclaration(TypeInfoIEnumrableType, "ControllerTypes")
                                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(TypesVariable))
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))),

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
                            //public static IMvcBuilder AddSourceGeneratorProviders(this IMvcBuilder builder)
                            SyntaxFactory.MethodDeclaration(MvcBuilderType, "AddSourceGeneratorProviders")
                                .AddParameterListParameters(
                                    SyntaxFactory.Parameter(BuilderVariable.Identifier)
                                        .WithType(MvcBuilderType)
                                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ThisKeyword)))
                                )
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                                .WithBody(SyntaxFactory.Block(
                                    //var mvcSourceGenContext = new MvcSourceGenContext();
                                    SyntaxFactory.LocalDeclarationStatement(
                                        SyntaxFactory.VariableDeclaration(MvcSourceGenContextType)
                                            .AddVariables(
                                                SyntaxFactory.VariableDeclarator(ContextVariable.Identifier)
                                                    .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ImplicitObjectCreationExpression()))
                                            )
                                     ),
                                    
                                    //  return builder.AddSourceGeneratorProviders
                                    SyntaxFactory.ReturnStatement(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                BuilderVariable,
                                                SyntaxFactory.IdentifierName("AddSourceGeneratorProviders")
                                            ))
                                        .WithArgumentList(SyntaxFactory.ArgumentList().AddArguments(
                                            // ISourceGenControllerTypeProvider
                                            SyntaxFactory.Argument(ContextVariable),
                                            // ISourceGenModelMetadataProvider
                                            SyntaxFactory.Argument(ContextVariable)
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