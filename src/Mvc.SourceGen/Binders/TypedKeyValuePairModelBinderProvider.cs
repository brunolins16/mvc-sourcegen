namespace Mvc.SourceGen.Binders;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;

internal class TypedKeyValuePairModelBinderProvider : IModelBinderProvider
{
    private readonly KeyValuePairModelBinderProvider _reflectionBinderProvider = new();

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var modelType = context.Metadata.ModelType;
        if (modelType.IsGenericType &&
            modelType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
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
