namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

internal class SourceGenerationSpec
{
    public Dictionary<INamedTypeSymbol, SourceGenerationActionMethodSpec[]?> ControllerTypes { get; set; } = default!;
    public SourceGenerationModelSpec[] ModelTypes { get; set; } = default!;
}