namespace Mvc.SourceGen.Generators.Emitters;

using Microsoft.CodeAnalysis;

internal interface IEmitter
{
    public void Emit(SourceProductionContext context, SourceGenerationSpec spec);
}
