namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class WellKnownTypes
{
    // Namespaces

    public static readonly IdentifierNameSyntax SourceGenNamespace = SyntaxFactory.IdentifierName("Mvc.SourceGen");

    // Types
    public static readonly IdentifierNameSyntax Var = SyntaxFactory.IdentifierName("var");
    public static readonly IdentifierNameSyntax MvcSourceGenContext = SyntaxFactory.IdentifierName("MvcSourceGenContext");
    public static readonly IdentifierNameSyntax TypeInfo = SyntaxFactory.IdentifierName("TypeInfo");
    public static readonly IdentifierNameSyntax IEnumerable = SyntaxFactory.IdentifierName("IEnumerable");
    public static readonly GenericNameSyntax IEnumerableOfTypeInfo = SyntaxFactory.GenericName(IEnumerable.Identifier, SyntaxFactory.TypeArgumentList().AddArguments(TypeInfo));
    public static readonly IdentifierNameSyntax DynamicallyAccessedMemberTypes = SyntaxFactory.IdentifierName("DynamicallyAccessedMemberTypes");
    public static readonly IdentifierNameSyntax PropertyInfo = SyntaxFactory.IdentifierName("PropertyInfo");
    public static readonly ArrayTypeSyntax ArrayOfPropertyInfo = SyntaxFactory.ArrayType(PropertyInfo)
                                                                                       .AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier()
                                                                                        .AddSizes(SyntaxFactory.OmittedArraySizeExpression()));


}
