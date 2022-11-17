namespace Mvc.SourceGen;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

public interface ISourceGenControllerTypeProvider
{
    IEnumerable<TypeInfo> ControllerTypes { get; }

    bool TryGetControllerInfo(Type controllerType, [NotNullWhen(true)] out ControllerInfo? controllerInfo);
}