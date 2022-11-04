namespace Mvc.SourceGen.Web;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;
using Mvc.SourceGen.Web.Controllers;
using System.Diagnostics.CodeAnalysis;

public class SourceGenModelMetadataProvider : DefaultModelMetadataProvider
{
    /// <summary>
    /// Creates a new <see cref="DefaultModelMetadataProvider"/>.
    /// </summary>
    /// <param name="detailsProvider">The <see cref="ICompositeMetadataDetailsProvider"/>.</param>
    public SourceGenModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider)
        : base(detailsProvider)
    {
    }

    /// <summary>
    /// Creates a new <see cref="DefaultModelMetadataProvider"/>.
    /// </summary>
    /// <param name="detailsProvider">The <see cref="ICompositeMetadataDetailsProvider"/>.</param>
    /// <param name="optionsAccessor">The accessor for <see cref="MvcOptions"/>.</param>
    public SourceGenModelMetadataProvider(
        ICompositeMetadataDetailsProvider detailsProvider,
        IOptions<MvcOptions> optionsAccessor)
        : base(detailsProvider, optionsAccessor)
    {
    }

    protected override ModelMetadata CreateModelMetadata(DefaultMetadataDetails entry)
    {
        if (entry.Key.ModelType == typeof(Message))
        {
            return new MessageModelMetadata(this, DetailsProvider, entry, ModelBindingMessageProvider);
        }

        if (entry.Key.ModelType == typeof(MessageV2))
        {
            return new MessageV2ModelMetadata(this, DetailsProvider, entry, ModelBindingMessageProvider);
        }

        return base.CreateModelMetadata(entry);
    }
}

// TODO: We should implement source generate a model metadata for all discovered types
internal class MessageModelMetadata : ModelMetadata<Message>
{
    public MessageModelMetadata(
        IModelMetadataProvider provider, 
        ICompositeMetadataDetailsProvider detailsProvider, 
        DefaultMetadataDetails details, 
        DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(provider, detailsProvider, details, modelBindingMessageProvider)
    {
    }
}

internal class MessageV2ModelMetadata : ModelMetadata<MessageV2>
{
    public MessageV2ModelMetadata(
        IModelMetadataProvider provider,
        ICompositeMetadataDetailsProvider detailsProvider,
        DefaultMetadataDetails details,
        DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(provider, detailsProvider, details, modelBindingMessageProvider)
    {
    }
}

public class ModelMetadata<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : DefaultModelMetadata
{
    public ModelMetadata(
        IModelMetadataProvider provider, 
        ICompositeMetadataDetailsProvider detailsProvider, 
        DefaultMetadataDetails details, 
        DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(provider, detailsProvider, details, modelBindingMessageProvider)
    {
    }
}
