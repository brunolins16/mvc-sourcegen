using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Mvc.SourceGen.Web.Controllers;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

internal class SourceGenApplicationPart : ApplicationPart, IApplicationPartTypeProvider
{
    private readonly static TypeInfo[] _types;

    static SourceGenApplicationPart()
    {
        _types = new TypeInfo[1]
        {
           GetTypeInfo<HelloController>()
        };
    }

    internal static TypeInfo GetTypeInfo<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        => typeof(T).GetTypeInfo();

    /// <summary>
    /// Gets the name of the <see cref="ApplicationPart"/>.
    /// </summary>
    public override string Name => "Mvc.SourceGen"!;

    /// <inheritdoc />
    public IEnumerable<TypeInfo> Types => _types;
}