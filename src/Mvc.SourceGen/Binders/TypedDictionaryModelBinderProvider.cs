namespace Mvc.SourceGen.Binders;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;

internal class TypedDictionaryModelBinderProvider : IModelBinderProvider
{
    private readonly DictionaryModelBinderProvider _reflectionBinderProvider = new();

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (typeof(Dictionary<,>).IsAssignableFrom(context.Metadata.ModelType))
        {
            if (context.Metadata is ITypedModelMetadata typedMetadata)
            {
                return typedMetadata.CreateModelBinder(context);
            }

            return _reflectionBinderProvider.GetBinder(context);
        }

        return null;
    }
}
