namespace Mvc.SourceGen.Binders;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;

internal class TypedArrayModelBinderProvider : IModelBinderProvider
{
    private readonly ArrayModelBinderProvider _reflectionBinderProvider = new();

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Metadata.ModelType.IsArray)
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
