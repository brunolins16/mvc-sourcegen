namespace Mvc.SourceGen;

using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;

internal class SourceGenApplicationPart : ApplicationPart, IApplicationPartTypeProvider
{
    private readonly ISourceGenControllerTypeProvider? _controllerTypeProvider;

    public SourceGenApplicationPart(ISourceGenControllerTypeProvider? controllerTypeProvider = null)
    {
        _controllerTypeProvider = controllerTypeProvider;
    }

    public IEnumerable<TypeInfo> Types => _controllerTypeProvider?.ControllerTypes ?? Array.Empty<TypeInfo>();

    public override string Name => nameof(SourceGenApplicationPart);
}