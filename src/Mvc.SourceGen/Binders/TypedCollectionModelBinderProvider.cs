namespace Mvc.SourceGen.Binders;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;

internal class TypedCollectionModelBinderProvider : IModelBinderProvider
{
    private readonly CollectionModelBinderProvider _reflectionBinderProvider = new();

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Arrays are handled by another binder.
        if (context.Metadata.ModelType.IsArray)
        {
            return null;
        }

        if (context.Metadata.IsCollectionType || context.Metadata.IsEnumerableType)
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
