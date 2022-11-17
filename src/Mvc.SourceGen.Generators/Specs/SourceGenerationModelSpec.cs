namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

internal class SourceGenerationModelSpec
{
    public ITypeSymbol Type { get; set; } = default!;
    public bool IsArray { get; set; } = false;
    public bool IsCollection { get; set; } = false;
    public bool IsIEnumerable { get; set; } = false;
}

internal class SourceGenerationModelSpecComparer : IEqualityComparer<SourceGenerationModelSpec?>
{
    public static readonly SourceGenerationModelSpecComparer Default = new();

    internal SourceGenerationModelSpecComparer()
    {
    }

    /// <summary>
    /// Determines if two <see cref="ISymbol" /> instances are equal according to the rules of this comparer
    /// </summary>
    /// <param name="x">The first symbol to compare</param>
    /// <param name="y">The second symbol to compare</param>
    /// <returns>True if the symbols are equivalent</returns>
    public bool Equals(SourceGenerationModelSpec? x, SourceGenerationModelSpec? y)
    {
        if (x is null)
        {
            return y is null;
        }

        return x.Type.Equals(y?.Type, SymbolEqualityComparer.Default);
    }

    public int GetHashCode(SourceGenerationModelSpec? obj)
    {
#pragma warning disable RS1024 // Symbols should be compared for equality
        return obj?.Type.GetHashCode() ?? 0;
#pragma warning restore RS1024 // Symbols should be compared for equality
    }
}