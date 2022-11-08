namespace Mvc.SourceGen;

using System.Reflection;

public interface ISourceGenControllerTypeProvider
{
    IEnumerable<TypeInfo> ControllerTypes { get; }
}