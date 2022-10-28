using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Mvc.SourceGen;

public partial class SourceGenApplicationPart : ApplicationPart, IApplicationPartTypeProvider
{ 
    private readonly TypeInfo[] _types;

    public SourceGenApplicationPart()
    {
        var intialTypes = new List<TypeInfo>();
        AddTypeInfos(intialTypes);

        _types = intialTypes.ToArray();
    }

    partial void AddTypeInfos(List<TypeInfo> infos);

    /// <summary>
    /// Gets the name of the <see cref="ApplicationPart"/>.
    /// </summary>
    public override string Name => "Mvc.SourceGen"!;
 
    /// <inheritdoc />
    public IEnumerable<TypeInfo> Types => _types;
}
