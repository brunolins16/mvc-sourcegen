namespace Mvc.SourceGen.Generators;

using Microsoft.CodeAnalysis;
using Mvc.SourceGen.Generators.Emitters;

public sealed partial class MvcGenerator
{
    internal class Emitter
    {
        private readonly SourceProductionContext _context;
        private readonly SourceGenerationSpec _spec;
        private readonly IEmitter[] _emitters;

        public Emitter(SourceProductionContext context, SourceGenerationSpec spec)
        {
            _context = context;
            _spec = spec;
            _emitters = new IEmitter[] {
                new ModelMetadataEmitter(),
                new MvcBuilderExtensionsEmitter(),
                new ControllerTypeProviderEmitter()
            };
        }

        public void Emit()
        {
            foreach (var emitter in _emitters)
            {
                emitter.Emit(_context, _spec);
            }
        }
    }

}