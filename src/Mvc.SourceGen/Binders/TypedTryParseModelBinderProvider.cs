namespace Mvc.SourceGen.Binders;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;

internal class TypedTryParseModelBinderProvider : IModelBinderProvider
{
    private readonly TryParseModelBinderProvider _reflectionBinderProvider = new();

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var binder = _reflectionBinderProvider.GetBinder(context);

        //TODO: IsParseableType is internal, so, for now hacking 😂
        if (binder != null)
        {
            if (context.Metadata is ITypedModelMetadata typedMetadata)
            {
                return typedMetadata.CreateModelBinder(context);
            }
        }

        return binder;
    }
}
