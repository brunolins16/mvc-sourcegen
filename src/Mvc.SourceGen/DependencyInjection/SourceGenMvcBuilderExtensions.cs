namespace Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Mvc.SourceGen;
using Mvc.SourceGen.Binders;
using System.Linq;

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

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, SourceGenMvcOptionsSetup>());
        }


        if (controllerTypeProvider != null)
        {
            builder = builder.ConfigureApplicationPartManager(appPartManager
                => appPartManager.ApplicationParts.Add(new SourceGenApplicationPart(controllerTypeProvider)));

            _ = builder.Services.AddSingleton<ISourceGenControllerTypeProvider>(controllerTypeProvider);
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, SourceGenApplcationModelProvider>());
        }

        return builder;
    }
}

internal sealed class SourceGenMvcOptionsSetup : IConfigureOptions<MvcOptions>
{
    public void Configure(MvcOptions options)
    {
        options.ModelBinderProviders.Replace<ArrayModelBinderProvider, TypedArrayModelBinderProvider>();
        options.ModelBinderProviders.Replace<CollectionModelBinderProvider, TypedCollectionModelBinderProvider>();
        options.ModelBinderProviders.Replace<DictionaryModelBinderProvider, TypedDictionaryModelBinderProvider>();
        options.ModelBinderProviders.Replace<KeyValuePairModelBinderProvider, TypedKeyValuePairModelBinderProvider>();
        options.ModelBinderProviders.Replace<TryParseModelBinderProvider, TypedTryParseModelBinderProvider>();
    }
}

internal static class ListExtensions
{
    public static void Replace<TOld, TNew>(this IList<IModelBinderProvider> modelBinderProviders)
        where TNew : IModelBinderProvider, new()
        where TOld : IModelBinderProvider
    {
        var currentProvider = modelBinderProviders.OfType<TOld>().FirstOrDefault();

        if (currentProvider != null)
        {
            var currentIndex = modelBinderProviders.IndexOf(currentProvider);
            modelBinderProviders.RemoveAt(currentIndex);
            modelBinderProviders.Insert(currentIndex, new TNew());
        }
    }

}