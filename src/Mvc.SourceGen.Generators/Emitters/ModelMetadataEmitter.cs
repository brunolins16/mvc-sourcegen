namespace Mvc.SourceGen.Generators.Emitters;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

internal class ModelMetadataEmitter : IEmitter
{
    public void Emit(SourceProductionContext context, SourceGenerationSpec spec)
    {
        // Includes all detected types + MvcSourceGenContext
        var classDeclarations = new MemberDeclarationSyntax[spec.ModelTypes.Length];
        var typeMapping = new Dictionary<string, IdentifierNameSyntax>(StringComparer.OrdinalIgnoreCase);

        // TODO: Check if it works with generics + records + nullables
        for (int i = 0; i < spec.ModelTypes.Length; i++)
        {
            var modelType = spec.ModelTypes[i];
            const string suffix = "ModelMetadata";

            IdentifierNameSyntax metadataType;

            if (modelType.Type.SpecialType != SpecialType.None)
            {
                metadataType = SyntaxFactory.IdentifierName($"{modelType.Type.SpecialType}{suffix}");
            }
            else
            {
                // TODO: HACK 😅 Fix + performance
                metadataType = SyntaxFactory.IdentifierName(
                                    $"{modelType.Type.ToDisplayString()}{suffix}"
                                    .Replace(".", "")
                                    .Replace(",", "")
                                    .Replace("?", "Nullable")
                                    .Replace(">", "")
                                    .Replace("<", ""));

            }

            typeMapping[modelType.Type.ToDisplayString()] = metadataType;

            if (modelType.OriginalType != null)
            {
                typeMapping[modelType.OriginalType.ToDisplayString()] = metadataType;
            }

            classDeclarations[i] = SyntaxFactory.ClassDeclaration(metadataType.Identifier)
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.FileKeyword)))
                    .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(new BaseTypeSyntax[]
                    {
                            SyntaxFactory.SimpleBaseType(SyntaxFactory.GenericName(WellKnownTypes.DefaultModelMetadata.Identifier, SyntaxFactory.TypeArgumentList().AddArguments(SyntaxFactory.IdentifierName(modelType.Type.ToDisplayString()))))
                    })))
                    .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                    {
                            //public {name}(
                            //  ISourceGenContext sourceGenContext,
                            //  IModelMetadataProvider provider,
                            //  ICompositeMetadataDetailsProvider detailsProvider,
                            //  DefaultMetadataDetails details,
                            //  DefaultModelBindingMessageProvider modelBindingMessageProvider)
                            SyntaxFactory.ConstructorDeclaration(metadataType.Identifier)
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                .AddParameterListParameters(
                                    SyntaxFactory.Parameter(KnownVariables.SourceGenContextVariable.Identifier).WithType(WellKnownTypes.ISourceGenContext),
                                    SyntaxFactory.Parameter(KnownVariables.ProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("IModelMetadataProvider")),
                                    SyntaxFactory.Parameter(KnownVariables.DetailsProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("ICompositeMetadataDetailsProvider")),
                                    SyntaxFactory.Parameter(KnownVariables.DetailsVariable.Identifier).WithType(SyntaxFactory.IdentifierName("DefaultMetadataDetails")),
                                    SyntaxFactory.Parameter(KnownVariables.ModelBindingMessageProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("DefaultModelBindingMessageProvider"))
                                )
                                // : base(sourceGenContext, provider, detailsProvider, details, modelBindingMessageProvider)
                                .WithInitializer(SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer).AddArgumentListArguments(
                                    SyntaxFactory.Argument(KnownVariables.SourceGenContextVariable),
                                    SyntaxFactory.Argument(KnownVariables.ProviderVariable),
                                    SyntaxFactory.Argument(KnownVariables.DetailsProviderVariable),
                                    SyntaxFactory.Argument(KnownVariables.DetailsVariable),
                                    SyntaxFactory.Argument(KnownVariables.ModelBindingMessageProviderVariable)
                                    ))
                                // {}
                                .WithBody(SyntaxFactory.Block()),
                    }))
                    .WithPropertyInitMethod(modelType)
                    .WithCtorInitMethod(modelType.Type)
                    .WithGetBinderMethod(modelType)
                    .WithLeadingTrivia(SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, $"// {spec.ModelTypes[i].Type.MetadataName}"));
        }

        // namespace Mvc.SourceGen ?? App namespace
        var declaration = SyntaxFactory.NamespaceDeclaration(WellKnownTypes.SourceGenNamespace)
            .WithLeadingTrivia(SyntaxFactory.Trivia(SyntaxFactory.NullableDirectiveTrivia(SyntaxFactory.Token(SyntaxKind.EnableKeyword), true)))
            .WithUsings(SyntaxFactory.List(new UsingDirectiveSyntax[]
            {
                    // using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Mvc.ModelBinding.Metadata")),
                    // using Microsoft.AspNetCore.Mvc.ModelBinding;
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Mvc.ModelBinding")),
                    // using System.Diagnostics.CodeAnalysis; 
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Diagnostics.CodeAnalysis"))
            }))
            .WithModelMetadataProviderContext(typeMapping)
            .AddMembers(classDeclarations);

        context.AddSource(
            "MvcSourceGenContext.ModelMetadataProvider.g.cs",
            declaration
            .NormalizeWhitespace()
            .GetText(encoding: Encoding.UTF8));
    }
}

file static class KnownVariables
{
    public static readonly IdentifierNameSyntax ProviderVariable = SyntaxFactory.IdentifierName("provider");
    public static readonly IdentifierNameSyntax DetailsVariable = SyntaxFactory.IdentifierName("details");
    public static readonly IdentifierNameSyntax ModelBindingMessageProviderVariable = SyntaxFactory.IdentifierName("modelBindingMessageProvider");
    public static readonly IdentifierNameSyntax SourceGenContextVariable = SyntaxFactory.IdentifierName("sourceGenContext");
    public static readonly IdentifierNameSyntax DetailsProviderVariable = SyntaxFactory.IdentifierName("detailsProvider");
    public static readonly IdentifierNameSyntax EntryVariable = SyntaxFactory.IdentifierName("entry");
    public static readonly IdentifierNameSyntax ModelMetadataVariable = SyntaxFactory.IdentifierName("modelMetadata");
    public static readonly IdentifierNameSyntax ModelVariable = SyntaxFactory.IdentifierName("model");
    public static readonly IdentifierNameSyntax ContextVariable = SyntaxFactory.IdentifierName("context");
}

file static class SyntaxFactoryExtensions
{
    public static NamespaceDeclarationSyntax WithModelMetadataProviderContext(this NamespaceDeclarationSyntax namespaceDeclarationSyntax, Dictionary<string, IdentifierNameSyntax> typeMapping)
    {
        var expressions = new List<StatementSyntax>(typeMapping.Count + 2)
            {
                // modelMetadata = null;
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        KnownVariables.ModelMetadataVariable,
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
                                KnownVariables.EntryVariable,
                                SyntaxFactory.IdentifierName("Key")
                                ),
                            SyntaxFactory.IdentifierName("ModelType")
                            ),
                        SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(item.Key))),
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            KnownVariables.ModelMetadataVariable,
                            SyntaxFactory.ObjectCreationExpression(item.Value)
                            .AddArgumentListArguments(
                                SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                                SyntaxFactory.Argument(KnownVariables.ProviderVariable),
                                SyntaxFactory.Argument(KnownVariables.DetailsProviderVariable),
                                SyntaxFactory.Argument(KnownVariables.EntryVariable),
                                SyntaxFactory.Argument(KnownVariables.ModelBindingMessageProviderVariable)
                                )
                            ))
                    ));
        }

        // return modelMetadata != null
        expressions.Add(SyntaxFactory.ReturnStatement(
            SyntaxFactory.BinaryExpression(
                SyntaxKind.NotEqualsExpression,
                KnownVariables.ModelMetadataVariable,
                SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))));

        // internal partial class MvcSourceGenContext : ISourceGenContext
        return namespaceDeclarationSyntax.AddMembers(SyntaxFactory.ClassDeclaration(WellKnownTypes.MvcSourceGenContext.Identifier)
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
            .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(new BaseTypeSyntax[]
            {
                    SyntaxFactory.SimpleBaseType(WellKnownTypes.ISourceGenContext),
            })))
            .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
            {
                    // bool TryCreateModelMetadata(, , , , );
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), "TryCreateModelMetadata")
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                        .AddParameterListParameters(
                            // DefaultMetadataDetails entry
                            SyntaxFactory.Parameter(KnownVariables.EntryVariable.Identifier).WithType(SyntaxFactory.IdentifierName("DefaultMetadataDetails")),
                            // IModelMetadataProvider provider
                            SyntaxFactory.Parameter(KnownVariables.ProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("IModelMetadataProvider")),
                            // ICompositeMetadataDetailsProvider detailsProvider
                            SyntaxFactory.Parameter(KnownVariables.DetailsProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("ICompositeMetadataDetailsProvider")),
                            // DefaultModelBindingMessageProvider modelBindingMessageProvider
                            SyntaxFactory.Parameter(KnownVariables.ModelBindingMessageProviderVariable.Identifier).WithType(SyntaxFactory.IdentifierName("DefaultModelBindingMessageProvider")),
                            //[NotNullWhen(true)] out ModelMetadata? modelMetadata
                            SyntaxFactory.Parameter(KnownVariables.ModelMetadataVariable.Identifier)
                                .WithType(SyntaxFactory.IdentifierName("ModelMetadata?"))
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.OutKeyword)))
                        )
                        //  => null;
                        .WithBody(SyntaxFactory.Block(expressions))
            })));
    }

    public static ClassDeclarationSyntax WithPropertyInitMethod(this ClassDeclarationSyntax classDeclarationSyntax, SourceGenerationModelSpec modelType)
    {
        var properties = GetProperties(modelType.Type);

        if (properties.Length == 0 || modelType.Type.SpecialType != SpecialType.None || modelType.IsIEnumerable || modelType.IsCollection)
        {
            return classDeclarationSyntax;
        }

        var isValueType = modelType.Type.IsValueType;

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
                                SyntaxFactory.ParenthesizedExpression(SyntaxFactory.CastExpression(SyntaxFactory.IdentifierName(modelType.Type.ToDisplayString()), SyntaxFactory.IdentifierName("obj"))),
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
                    properties[i].IsReadOnly || properties[i].SetMethod!.IsInitOnly ?
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
                                SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName($"System.Runtime.CompilerServices.Unsafe.Unbox<{modelType.Type.ToDisplayString()}>"))
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
                                SyntaxFactory.ParenthesizedExpression(SyntaxFactory.CastExpression(SyntaxFactory.IdentifierName(modelType.Type.ToDisplayString()), SyntaxFactory.IdentifierName("obj"))),
                                SyntaxFactory.IdentifierName(properties[i].Name)),
                            SyntaxFactory.CastExpression(
                                SyntaxFactory.IdentifierName(properties[i].Type.ToDisplayString()),
                                SyntaxFactory.PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression, SyntaxFactory.IdentifierName("value")))
                        )
                );


            methodBlock = methodBlock.AddStatements(

                    //var propertyInfo{i} = declaredType.GetProperty("");
                    SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(WellKnownTypes.Var).AddVariables(
                        SyntaxFactory.VariableDeclarator($"propertyInfo{i}")
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(modelType.Type.ToDisplayString())),
                                        SyntaxFactory.IdentifierName("GetProperty"))
                                    )
                                .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(properties[i].Name))))
                        )))),

                    //var propertyInfo0_key = ModelMetadataIdentity.ForProperty(propertyInfo0!, typeof(string), declaredType);
                    SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(WellKnownTypes.Var).AddVariables(
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
                                    SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(modelType.Type.ToDisplayString()))))
                        )))),

                    //var propertyInfo0_attributes = ModelAttributes.GetAttributesForProperty(declaredType, propertyInfo0!, typeof(string));
                    SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(WellKnownTypes.Var).AddVariables(
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
                                    SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(modelType.Type.ToDisplayString()))),
                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName($"propertyInfo{i}")),
                                    SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(propertyType.ToDisplayString()))))
                        )))),

                    // var propertyInfo0_entry = new DefaultMetadataDetails(propertyInfo{i}_key, propertyInfo0_attributes)
                    SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(WellKnownTypes.Var).AddVariables(
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

        methodBlock = methodBlock.AddStatements(
            SyntaxFactory.ReturnStatement(SyntaxFactory.ImplicitArrayCreationExpression(SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression).AddExpressions(metadataInitializers))));

        //protected override ModelMetadata[] PropertiesInit()
        return classDeclarationSyntax.AddMembers(SyntaxFactory.MethodDeclaration(WellKnownTypes.ModelMetadataArray, "PropertiesInit")
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
            .WithBody(methodBlock));
    }

    public static ClassDeclarationSyntax WithCtorInitMethod(this ClassDeclarationSyntax classDeclarationSyntax, ITypeSymbol modelType)
    {
        if (!modelType.IsRecord || modelType.IsAbstract || modelType.IsValueType)
        {
            return classDeclarationSyntax;
        }

        var properties = GetProperties(modelType);

        if (modelType is not INamedTypeSymbol namedModelType)
        {
            return classDeclarationSyntax;
        }

        var constructor = GetConstructor(namedModelType, properties);

        if (constructor == null)
        {
            return classDeclarationSyntax;
        }

        // new[] {  }
        ImplicitArrayCreationExpressionSyntax ctorParams = SyntaxFactory.ImplicitArrayCreationExpression(SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression));

        var creationExpression = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName(modelType.ToDisplayString()));
        for (int i = 0; i < constructor.Parameters.Length; i++)
        {
            var ctorType = constructor.Parameters[i].Type;

            if (ctorType.IsReferenceType)
            {
                ctorType = ctorType.WithNullableAnnotation(NullableAnnotation.None);
            }

            // typeof(string)
            ctorParams = ctorParams.AddInitializerExpressions(SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(ctorType.ToDisplayString())));

            // ({type})args[i]
            // TODO: Nullable
            creationExpression = creationExpression.AddArgumentListArguments(
                SyntaxFactory.Argument(
                    SyntaxFactory.NameColon(constructor.Parameters[i].Name),
                    SyntaxFactory.Token(SyntaxKind.None),
                    SyntaxFactory.CastExpression(
                        SyntaxFactory.IdentifierName(constructor.Parameters[i].Type.ToDisplayString()),
                        SyntaxFactory.ElementAccessExpression(
                            SyntaxFactory.IdentifierName("args"),
                            SyntaxFactory.BracketedArgumentList()
                            .AddArguments(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        SyntaxFactory.Literal(i)))
                            )))
            ));
        }

        //protected override ModelMetadata? CtorInit()
        return classDeclarationSyntax.AddMembers(SyntaxFactory.MethodDeclaration(SyntaxFactory.NullableType(WellKnownTypes.ModelMetadata), "CtorInit")
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
            .WithExpressionBody(
                // => CtorInit(ctorParams, ctorInvoker)
                SyntaxFactory.ArrowExpressionClause(
                    SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("CtorInit"))
                    .AddArgumentListArguments(
                        // new[] { typeof(string), typeof(string) }
                        SyntaxFactory.Argument(ctorParams),
                        SyntaxFactory.Argument(
                            // static (args) => new MessageV2(Text: (string)args[0]!, Name: (string?)args[1]))
                            SyntaxFactory.ParenthesizedLambdaExpression()
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                            .AddParameterListParameters(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier("args")))
                            .WithExpressionBody(creationExpression))
                    )
                ))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

    }

    private enum Test { Test1, Test2 }

    public static ClassDeclarationSyntax WithGetBinderMethod(this ClassDeclarationSyntax classDeclarationSyntax, SourceGenerationModelSpec modelType)
    {
        if (modelType.Type.SpecialType == SpecialType.System_String)
        {
            return classDeclarationSyntax;
        }

        ExpressionSyntax? creator = null;

        if (modelType.IsParsable)
        {
            var tryParseInvocation = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, 
                SyntaxFactory.IdentifierName(modelType.TryParseMethod!.ContainingType.ToDisplayString()), SyntaxFactory.IdentifierName("TryParse")))
                .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("tempSourceString")));

            tryParseInvocation = (modelType.IsEnum, modelType.TryParseMethod!.Parameters.Length) switch
            {
                // (Enum.TryParse(tempSourceString,***** true ******, out [modelType] parsedValue))
                (true, _) =>  tryParseInvocation.AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression))),
                // ([modeltype].TryParse(tempSourceString, **** valueProviderResult.Culture *****, out [modelType] parsedValue))
                (false, 3) => tryParseInvocation.AddArgumentListArguments(
                    SyntaxFactory.Argument(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, KnownVariables.ProviderVariable, SyntaxFactory.IdentifierName("Culture")))),
                _ => tryParseInvocation
            };

            //  *****out [modelType] parsedValue******
            tryParseInvocation = tryParseInvocation.AddArgumentListArguments(
                SyntaxFactory.Argument(
                    null, 
                    SyntaxFactory.Token(SyntaxKind.OutKeyword), 
                    SyntaxFactory.DeclarationExpression(
                        SyntaxFactory.IdentifierName(modelType.Type.ToDisplayString()),
                        SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier("parsedValue"))))
            );

            // private object? ParseOperation(ValueProviderResult provider, ModelBindingContext context)
            classDeclarationSyntax = classDeclarationSyntax.AddMembers(SyntaxFactory.MethodDeclaration(SyntaxFactory.NullableType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword))), "ParseOperation")
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(KnownVariables.ProviderVariable.Identifier).WithType(WellKnownTypes.ValuResultProvider),
                    SyntaxFactory.Parameter(KnownVariables.ContextVariable.Identifier).WithType(WellKnownTypes.ModelBindingContext))
                .WithBody(SyntaxFactory.Block(

                    // var tempSourceString = provider.FirstValue;
                    SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(WellKnownTypes.Var).AddVariables(
                        SyntaxFactory.VariableDeclarator("tempSourceString")
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,KnownVariables.ProviderVariable,SyntaxFactory.IdentifierName("FirstValue"))))
                        )),

                    // object? model = null;
                    SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(SyntaxFactory.NullableType(WellKnownTypes.Object)).AddVariables(
                        SyntaxFactory.VariableDeclarator(KnownVariables.ModelVariable.Identifier)
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)))
                        )),

                    // if ([modeltype].TryParse(tempSourceString, [valueProviderResult.Culture,] out [modelType] parsedValue))
                    SyntaxFactory.IfStatement(tryParseInvocation, SyntaxFactory.Block(
                        // model = (object)parsedValue;
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression, 
                                KnownVariables.ModelVariable, 
                                SyntaxFactory.CastExpression(WellKnownTypes.Object, SyntaxFactory.IdentifierName("parsedValue"))
                            )),

                        // context.Result = ModelBindingResult.Success(model);
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, KnownVariables.ContextVariable, SyntaxFactory.IdentifierName("Result")),
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, 
                                        SyntaxFactory.IdentifierName("ModelBindingResult"), 
                                        SyntaxFactory.IdentifierName("Success"))
                                    ).AddArgumentListArguments(SyntaxFactory.Argument(KnownVariables.ModelVariable))
                            ))
                        ))
                    .WithElse(SyntaxFactory.ElseClause(SyntaxFactory.Block(
                        SyntaxFactory.ExpressionStatement(
                            // context.ModelState.TryAddModelError(context.ModelName, new FormatException(), context.ModelMetadata);
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, KnownVariables.ContextVariable, SyntaxFactory.IdentifierName("ModelState")),
                                    SyntaxFactory.IdentifierName("TryAddModelError")
                                    ))
                            .AddArgumentListArguments(
                                //context.ModelName
                                SyntaxFactory.Argument(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, KnownVariables.ContextVariable, SyntaxFactory.IdentifierName("ModelName"))),
                                //new FormatException()
                                SyntaxFactory.Argument(SyntaxFactory.ObjectCreationExpression(WellKnownTypes.FormatException, SyntaxFactory.ArgumentList(), null)),
                                //context.ModelMetadata
                                SyntaxFactory.Argument(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, KnownVariables.ContextVariable, SyntaxFactory.IdentifierName("ModelMetadata")))
                            )
                     )))),

                    // return model;
                    SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("model"))))
                );

            // TryParseModelBinder
            // public TryParseModelBinder(Func<ValueProviderResult, ModelBindingContext, object?> tryParseOperation, ILoggerFactory loggerFactory)
            creator = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Mvc.ModelBinding.Binders.TryParseModelBinder"))
                .AddArgumentListArguments(
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("ParseOperation")),
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("loggerFactory"))
                );
        }
        if (modelType.IsArray && modelType.Type is IArrayTypeSymbol arrayTypeSymbol && arrayTypeSymbol.ElementType != null)
        {
            // context.CreateBinder(context.Metadata.ElementMetadata)
            var elementBinder = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("context"),
                    SyntaxFactory.IdentifierName("CreateBinder")
                ))
                .AddArgumentListArguments(
                    // context.Metadata.ElementMetadata
                    SyntaxFactory.Argument(SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("context"),
                            SyntaxFactory.IdentifierName("Metadata")),
                        SyntaxFactory.IdentifierName("ElementMetadata"))
                    )
                );

            // ArrayModelBinder<T>
            // public ArrayModelBinder(IModelBinder elementBinder, ILoggerFactory loggerFactory, bool allowValidatingTopLevelNodes, MvcOptions mvcOptions)
            creator = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Microsoft.AspNetCore.Mvc.ModelBinding.Binders.ArrayModelBinder"), SyntaxFactory.TypeArgumentList().AddArguments(SyntaxFactory.IdentifierName(arrayTypeSymbol.ElementType.ToDisplayString()))))
                .AddArgumentListArguments(
                    SyntaxFactory.Argument(elementBinder),
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("loggerFactory")),
                    SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)),
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("options"))
                );
        }
        else if (modelType.IsCollection || modelType.IsIEnumerable)
        {
            //TODO: Check if can assign to ILIST when IEnumerable

            var typeArgument = SyntaxFactory.IdentifierName((modelType.Type as INamedTypeSymbol)!.TypeArguments[0].ToDisplayString());

            // context.CreateBinder(context.MetadataProvider.GetMetadataForType(elementType))
            var elementBinder = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("context"),
                    SyntaxFactory.IdentifierName("CreateBinder")
                ))
                .AddArgumentListArguments(
                    // context.Metadata.ElementMetadata
                    SyntaxFactory.Argument(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("context"),
                                    SyntaxFactory.IdentifierName("MetadataProvider")
                                ),
                                SyntaxFactory.IdentifierName("GetMetadataForType")))
                        .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(typeArgument)))
                        ));

            // CollectionModelBinder<T>
            //public CollectionModelBinder(IModelBinder elementBinder, ILoggerFactory loggerFactory, bool allowValidatingTopLevelNodes, MvcOptions mvcOptions)
            creator = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Microsoft.AspNetCore.Mvc.ModelBinding.Binders.CollectionModelBinder"), SyntaxFactory.TypeArgumentList().AddArguments(typeArgument)))
                .AddArgumentListArguments(
                    SyntaxFactory.Argument(elementBinder),
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("loggerFactory")),
                    SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)),
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("options"))
                );
        }
        //TODO: IsDictionary
        //TODO: IsKeyValuePair

        if (creator != null)
        {
            // protected virtual IModelBinder? CreateModelBinder(ModelBinderProviderContext context, ILoggerFactory loggerFactory, MvcOptions options) => new IModelBinder();
            classDeclarationSyntax = classDeclarationSyntax.AddMembers(SyntaxFactory.MethodDeclaration(SyntaxFactory.NullableType(WellKnownTypes.IModelBinder), "CreateModelBinder")
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("context")).WithType(SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Mvc.ModelBinding.ModelBinderProviderContext")),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("loggerFactory")).WithType(SyntaxFactory.IdentifierName("Microsoft.Extensions.Logging.ILoggerFactory")),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("options")).WithType(SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Mvc.MvcOptions"))
                )
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(creator))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
        }

        return classDeclarationSyntax;
    }

    private static IMethodSymbol? GetConstructor(INamedTypeSymbol modelType, IPropertySymbol[] properties)
    {
        foreach (var constructor in modelType.Constructors)
        {
            var ctorParameters = constructor.Parameters;

            if (properties.Length == ctorParameters.Length)
            {
                var match = true;
                for (int i = 0; i < properties.Length; i++)
                {
                    if (!properties[i].Type.Equals(ctorParameters[i].Type, SymbolEqualityComparer.IncludeNullability))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return constructor;
                }
            }
        }

        return null;
    }

    private static IPropertySymbol[] GetProperties(ITypeSymbol modelType)
    {
        List<IPropertySymbol> properties = default!;
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
}
