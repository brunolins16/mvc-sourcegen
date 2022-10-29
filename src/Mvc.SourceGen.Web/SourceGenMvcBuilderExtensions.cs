namespace Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Mvc.SourceGen.Web;

public static class SourceGenMvcBuilderExtensions
{
    public static IMvcBuilder AddSourceGenControllers(this IMvcBuilder builder)
    {
        return builder.ConfigureApplicationPartManager(appPartManager => 
        {
            appPartManager.ApplicationParts.Add(new SourceGenApplicationPart());
        });
    }
}