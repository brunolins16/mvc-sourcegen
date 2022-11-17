namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis;

internal class SourceGenerationActionMethodSpec
{
    public IMethodSymbol Method { get; set; } = default!;
    public ITypeSymbol? AwaiterType { get; set; }
    public ITypeSymbol? AsyncResultType { get; set; }

    public bool IsAsync => AwaiterType != null && AsyncResultType != null;
}