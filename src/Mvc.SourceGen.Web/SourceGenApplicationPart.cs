using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Mvc.SourceGen.Web.Controllers;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IMvcBuilder AddSourceGenControllers(this IServiceCollection services)
    {
        var manager = new ApplicationPartManager();
        manager.ApplicationParts.Add(new SourceGenApplicationPart());

        services.AddSingleton(manager);

        return services.AddControllers();
    }
}

public partial class SourceGenApplicationPart : ApplicationPart, IApplicationPartTypeProvider
{ 
    private readonly static TypeInfo[] _types;

    static SourceGenApplicationPart()
    {        
        _types = new TypeInfo[1] 
        {
            typeof(HelloController).GetTypeInfo()
        };
    }

    /// <summary>
    /// Gets the name of the <see cref="ApplicationPart"/>.
    /// </summary>
    public override string Name => "Mvc.SourceGen"!;
 
    /// <inheritdoc />
    public IEnumerable<TypeInfo> Types => _types;
}

