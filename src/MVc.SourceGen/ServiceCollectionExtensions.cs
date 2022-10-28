using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMvcSourceGen(this IServiceCollection services)
    {
        var manager = new ApplicationPartManager();
        //manager.ApplicationParts.Add(new SourceGenApplicationPart());

        services.AddSingleton(manager);

        return services;
    }
}