namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

internal static class TypeSyntaxConstants
{
    // Namespaces
    public static readonly IdentifierNameSyntax DINamespace = SyntaxFactory.IdentifierName("Microsoft.Extensions.DependencyInjection");
    public static readonly IdentifierNameSyntax SourceGenNamespace = SyntaxFactory.IdentifierName("Mvc.SourceGen");

    // Types
    public static readonly IdentifierNameSyntax VarType = SyntaxFactory.IdentifierName("var");
    public static readonly IdentifierNameSyntax MvcBuilderType = SyntaxFactory.IdentifierName("IMvcBuilder");
    public static readonly IdentifierNameSyntax MvcSourceGenContextType = SyntaxFactory.IdentifierName("MvcSourceGenContext");
    public static readonly IdentifierNameSyntax ControllerTypeProviderType = SyntaxFactory.IdentifierName("ISourceGenControllerTypeProvider");
    public static readonly IdentifierNameSyntax ModelMetadataProviderType = SyntaxFactory.IdentifierName("ISourceGenModelMetadataProvider");
    public static readonly IdentifierNameSyntax TypeInfoType = SyntaxFactory.IdentifierName("TypeInfo");
    public static readonly IdentifierNameSyntax EnumerableType = SyntaxFactory.IdentifierName("IEnumerable");
    public static readonly GenericNameSyntax TypeInfoIEnumrableType = SyntaxFactory.GenericName(EnumerableType.Identifier, SyntaxFactory.TypeArgumentList().AddArguments(TypeInfoType));
    public static readonly IdentifierNameSyntax DynamicallyAccessedMemberTypesType = SyntaxFactory.IdentifierName("DynamicallyAccessedMemberTypes");
    public static readonly IdentifierNameSyntax ModelMetadataType = SyntaxFactory.IdentifierName("ModelMetadata");
    public static readonly ArrayTypeSyntax ModelMetadataArrayType = SyntaxFactory.ArrayType(ModelMetadataType)
                                                                                       .AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier()
                                                                                        .AddSizes(SyntaxFactory.OmittedArraySizeExpression()));

    // variables
    public static readonly IdentifierNameSyntax BuilderVariable = SyntaxFactory.IdentifierName("builder");
    public static readonly IdentifierNameSyntax TypesVariable = SyntaxFactory.IdentifierName("_types");
    public static readonly IdentifierNameSyntax ContextVariable = SyntaxFactory.IdentifierName("mvcSourceGenContext");
    public static readonly IdentifierNameSyntax ProviderVariable = SyntaxFactory.IdentifierName("provider");
    public static readonly IdentifierNameSyntax DetailsProviderVariable = SyntaxFactory.IdentifierName("detailsProvider");
    public static readonly IdentifierNameSyntax DetailsVariable = SyntaxFactory.IdentifierName("details");
    public static readonly IdentifierNameSyntax ModelBindingMessageProviderVariable = SyntaxFactory.IdentifierName("modelBindingMessageProvider");
    public static readonly IdentifierNameSyntax ApplicationModelMetadataProviderVariable = SyntaxFactory.IdentifierName("applicationModelMetadataProvider");
    public static readonly IdentifierNameSyntax EntryVariable = SyntaxFactory.IdentifierName("entry");
    public static readonly IdentifierNameSyntax ModelMetadataVariable = SyntaxFactory.IdentifierName("modelMetadata");
}
