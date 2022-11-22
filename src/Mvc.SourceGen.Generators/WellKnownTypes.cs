namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class WellKnownTypes
{
    // Namespaces
    public static readonly IdentifierNameSyntax SourceGenNamespace = SyntaxFactory.IdentifierName("Mvc.SourceGen");

    // ASP.NET CORE types
    public static readonly IdentifierNameSyntax ISourceGenContext = SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Mvc.Infrastructure.ISourceGenContext");
    public static readonly IdentifierNameSyntax ControllerInfo = SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Mvc.Abstractions.ControllerInfo");
    public static readonly IdentifierNameSyntax ControllerActionInfo = SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Mvc.Abstractions.ControllerActionInfo");
    public static readonly ArrayTypeSyntax ControllerActionInfoArray = SyntaxFactory.ArrayType(ControllerActionInfo)
                                                                                       .AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier()
                                                                                        .AddSizes(SyntaxFactory.OmittedArraySizeExpression()));

    public static readonly IdentifierNameSyntax ValuResultProvider = SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult");
    public static readonly IdentifierNameSyntax ModelBindingContext = SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingContext");
    public static readonly IdentifierNameSyntax IModelBinder = SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Mvc.ModelBinding.IModelBinder");
    public static readonly IdentifierNameSyntax DefaultModelMetadata = SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadata");
    public static readonly IdentifierNameSyntax ModelMetadata = SyntaxFactory.IdentifierName("Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata");
    public static readonly ArrayTypeSyntax ModelMetadataArray = SyntaxFactory.ArrayType(ModelMetadata)
                                                                                       .AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier()
                                                                                        .AddSizes(SyntaxFactory.OmittedArraySizeExpression()));

    // Runtime types
    public static readonly IdentifierNameSyntax Var = SyntaxFactory.IdentifierName("var");
    public static readonly PredefinedTypeSyntax Object = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));
    public static readonly IdentifierNameSyntax IEnumerable = SyntaxFactory.IdentifierName("System.Collections.Generic.IEnumerable");
    public static readonly IdentifierNameSyntax DynamicallyAccessedMemberTypes = SyntaxFactory.IdentifierName("System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes");

    public static readonly IdentifierNameSyntax TypeInfo = SyntaxFactory.IdentifierName("System.Reflection.TypeInfo");
    public static readonly GenericNameSyntax IEnumerableOfTypeInfo = SyntaxFactory.GenericName(IEnumerable.Identifier, SyntaxFactory.TypeArgumentList().AddArguments(TypeInfo));

    public static readonly IdentifierNameSyntax PropertyInfo = SyntaxFactory.IdentifierName("System.Reflection.PropertyInfo");
    public static readonly ArrayTypeSyntax ArrayOfPropertyInfo = SyntaxFactory.ArrayType(PropertyInfo)
                                                                                       .AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier()
                                                                                        .AddSizes(SyntaxFactory.OmittedArraySizeExpression()));

    public static readonly IdentifierNameSyntax ICriticalNotifyCompletion = SyntaxFactory.IdentifierName("System.Runtime.CompilerServices.ICriticalNotifyCompletion");
    public static readonly IdentifierNameSyntax INotifyCompletion = SyntaxFactory.IdentifierName("System.Runtime.CompilerServices.INotifyCompletion");
    public static readonly IdentifierNameSyntax FormatException = SyntaxFactory.IdentifierName("System.FormatException");

    // Types
    public static readonly IdentifierNameSyntax MvcSourceGenContext = SyntaxFactory.IdentifierName("MvcSourceGenContext");


}
