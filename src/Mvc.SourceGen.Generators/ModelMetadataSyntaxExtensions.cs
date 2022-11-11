namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Reflection;
using System.Collections.Generic;
using System;

internal static class ModelMetadataSyntaxExtensions
{
    public static ClassDeclarationSyntax WithPropertyInitMethod(this ClassDeclarationSyntax classDeclarationSyntax, INamedTypeSymbol modelType)
    {
        var properties = GetProperties(modelType);

        if (properties.Length == 0 || modelType.SpecialType != SpecialType.None)
        {
            return classDeclarationSyntax;
        }

        var isValueType = modelType.IsValueType;

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
                    SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(TypeSyntaxConstants.VarType).AddVariables(
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
                    SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(TypeSyntaxConstants.VarType).AddVariables(
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
                    SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(TypeSyntaxConstants.VarType).AddVariables(
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
                    SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(TypeSyntaxConstants.VarType).AddVariables(
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
        return classDeclarationSyntax.AddMembers(SyntaxFactory.MethodDeclaration(TypeSyntaxConstants.ModelMetadataArrayType, "PropertiesInit")
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
            .WithBody(methodBlock));
    }

    public static ClassDeclarationSyntax WithCtorInitMethod(this ClassDeclarationSyntax classDeclarationSyntax, INamedTypeSymbol modelType)
    {
        if (!modelType.IsRecord || modelType.IsAbstract || modelType.IsValueType)
        {
            return classDeclarationSyntax;
        }

        var properties = GetProperties(modelType);
        var constructor = GetConstructor(modelType, properties);

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
        return classDeclarationSyntax.AddMembers(SyntaxFactory.MethodDeclaration(SyntaxFactory.NullableType(TypeSyntaxConstants.ModelMetadataType), "CtorInit")
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
    private static IPropertySymbol[] GetProperties(INamedTypeSymbol modelType)
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
}
