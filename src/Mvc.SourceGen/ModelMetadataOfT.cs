namespace Mvc.SourceGen;

using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

public abstract class ModelMetadata<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : DefaultModelMetadata
{
    private ModelMetadata[]? _propertiesCache;
    private ModelPropertyCollection? _propertyCollection;
    private readonly ISourceGenModelMetadataProvider _applicationModelMetadataProvider;
    private readonly IModelMetadataProvider _provider;
    private readonly ICompositeMetadataDetailsProvider _detailsProvider;
    private readonly DefaultModelBindingMessageProvider _modelBindingMessageProvider;

    public ModelMetadata(
        ISourceGenModelMetadataProvider applicationModelMetadataProvider,
        IModelMetadataProvider provider,
        ICompositeMetadataDetailsProvider detailsProvider,
        DefaultMetadataDetails details,
        DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(provider, detailsProvider, details, modelBindingMessageProvider)
    {
        _applicationModelMetadataProvider = applicationModelMetadataProvider;
        _provider = provider;
        _detailsProvider = detailsProvider;
        _modelBindingMessageProvider = modelBindingMessageProvider;
    }

    public override ModelPropertyCollection Properties => _propertyCollection ??= new(_propertiesCache ??= PropertiesInit());

    public override IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType)
        => _propertiesCache ??= PropertiesInit();

    protected abstract ModelMetadata[] PropertiesInit();

    protected ModelMetadata CreateMetadata(DefaultMetadataDetails details)
    {
        return _applicationModelMetadataProvider.TryCreateModelMetadata(details, _provider, _detailsProvider, _modelBindingMessageProvider, out var metadata)
            ? metadata!
            : throw new InvalidOperationException();
    }
}