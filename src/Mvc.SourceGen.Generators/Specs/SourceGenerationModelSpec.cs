namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis;

internal class SourceGenerationModelSpec
{
    public ITypeSymbol Type { get; set; } = default!;
    public ITypeSymbol? OriginalType { get; internal set; }
    public bool IsArray { get; set; } = false;
    public bool IsCollection { get; set; } = false;
    public bool IsIEnumerable { get; set; } = false;
    public bool IsParsable { get; set; } = false;
    public IMethodSymbol? TryParseMethod { get; internal set; }
    public bool IsEnum { get; internal set; }
}