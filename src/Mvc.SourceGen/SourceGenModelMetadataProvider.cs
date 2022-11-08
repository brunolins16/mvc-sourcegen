namespace Mvc.SourceGen;

using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System;

internal class SourceGenModelMetadataProvider : DefaultModelMetadataProvider
{
    private readonly ISourceGenModelMetadataProvider? _applicationModelMetadataProvider;

    /// <summary>
    /// Creates a new <see cref="DefaultModelMetadataProvider"/>.
    /// </summary>
    /// <param name="detailsProvider">The <see cref="ICompositeMetadataDetailsProvider"/>.</param>
    public SourceGenModelMetadataProvider(
        ICompositeMetadataDetailsProvider detailsProvider,
        ISourceGenModelMetadataProvider? applicationModelMetadataProvider = null)
        : base(detailsProvider)
    {
        _applicationModelMetadataProvider = applicationModelMetadataProvider;
    }

    /// <summary>
    /// Creates a new <see cref="DefaultModelMetadataProvider"/>.
    /// </summary>
    /// <param name="detailsProvider">The <see cref="ICompositeMetadataDetailsProvider"/>.</param>
    /// <param name="optionsAccessor">The accessor for <see cref="MvcOptions"/>.</param>
    public SourceGenModelMetadataProvider(
        ICompositeMetadataDetailsProvider detailsProvider,
        IOptions<MvcOptions> optionsAccessor,
        ISourceGenModelMetadataProvider? applicationModelMetadataProvider = null)
        : base(detailsProvider, optionsAccessor)
    {
        _applicationModelMetadataProvider = applicationModelMetadataProvider;
    }

    protected override ModelMetadata CreateModelMetadata(DefaultMetadataDetails entry)
        => _applicationModelMetadataProvider?.TryCreateModelMetadata(entry, this, DetailsProvider, ModelBindingMessageProvider, out var appModelMetadata) == true
            ? appModelMetadata!
            : base.CreateModelMetadata(entry);

}
