namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis;

internal class SourceGenerationSpec
{
    public ITypeSymbol[] ControllerTypes { get; set; }
    public INamedTypeSymbol[] ModelTypes { get; set; }
}