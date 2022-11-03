namespace Mvc.SourceGen.Web;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;
using Mvc.SourceGen.Web.Controllers;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

public class SourceGenModelMetadataProvider : DefaultModelMetadataProvider
{
    private readonly ConcurrentDictionary<ModelMetadataIdentity, ModelMetadataCacheEntry> _modelMetadataCache = new();
    private readonly Func<ModelMetadataIdentity, ModelMetadataCacheEntry> _cacheEntryFactory;

    /// <summary>
    /// Creates a new <see cref="DefaultModelMetadataProvider"/>.
    /// </summary>
    /// <param name="detailsProvider">The <see cref="ICompositeMetadataDetailsProvider"/>.</param>
    public SourceGenModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider)
        : base(detailsProvider)
    {
        _cacheEntryFactory = CreateCacheEntry;
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
        _cacheEntryFactory = CreateCacheEntry;
    }

    internal void ClearCache() => _modelMetadataCache.Clear();

    /// <inheritdoc />
    public override ModelMetadata GetMetadataForParameter(ParameterInfo parameter, Type modelType)
    {
        if (parameter == null)
        {
            throw new ArgumentNullException(nameof(parameter));
        }

        if (modelType == null)
        {
            throw new ArgumentNullException(nameof(modelType));
        }


        if (parameter.ParameterType == typeof(Message))
        {
            return GetCacheEntry(parameter, typeof(Message)).Metadata;
        }
        else
        {
            throw new InvalidOperationException($"Type {modelType.FullName} not supported");
        }
    }

    // TODO: Define the right member types that we need to access
    private ModelMetadataCacheEntry GetCacheEntry(ParameterInfo parameter, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type modelType)
    {
        return _modelMetadataCache.GetOrAdd(
            ModelMetadataIdentity.ForParameter(parameter, modelType),
            _cacheEntryFactory);
    }

    private ModelMetadataCacheEntry CreateCacheEntry(ModelMetadataIdentity key)
    {
        DefaultMetadataDetails details;

        //if (key.MetadataKind == ModelMetadataKind.Constructor)
        //{
        //    details = CreateConstructorDetails(key);
        //}
        if (key.MetadataKind == ModelMetadataKind.Parameter)
        {
            details = CreateParameterDetails(key);
        }
        //else if (key.MetadataKind == ModelMetadataKind.Property)
        //{
        //    details = CreateSinglePropertyDetails(key);
        //}
        else
        {
            details = CreateTypeDetails(key);
        }

        var metadata = CreateModelMetadata(details);
        return new ModelMetadataCacheEntry(metadata, details);
    }

    private readonly struct ModelMetadataCacheEntry
    {
        public ModelMetadataCacheEntry(ModelMetadata metadata, DefaultMetadataDetails details)
        {
            Metadata = metadata;
            Details = details;
        }

        public ModelMetadata Metadata { get; }

        public DefaultMetadataDetails Details { get; }
    }
}
