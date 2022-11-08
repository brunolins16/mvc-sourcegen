namespace Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;
using Mvc.SourceGen;

public static class SourceGenMvcBuilderExtensions
{
    public static IMvcBuilder AddSourceGeneratorProviders(
        this IMvcBuilder builder, 
        ISourceGenControllerTypeProvider? controllerTypeProvider = null,
        ISourceGenModelMetadataProvider? modelMetadataProvider = null)
    {
        if (modelMetadataProvider != null)
        {
            _ = builder.Services.AddSingleton<IModelMetadataProvider>((services)
                => new SourceGenModelMetadataProvider(
                    services.GetRequiredService<ICompositeMetadataDetailsProvider>(),
                    services.GetRequiredService<IOptions<MvcOptions>>(),
                    modelMetadataProvider));
        }

        return controllerTypeProvider != null
            ? builder.ConfigureApplicationPartManager(appPartManager => appPartManager.ApplicationParts.Add(new SourceGenApplicationPart(controllerTypeProvider)))
            : builder;
    }
}
