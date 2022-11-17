namespace Mvc.SourceGen;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public interface ITypedModelMetadata
{
    IModelBinder? CreateModelBinder(ModelBinderProviderContext context);
}

public abstract class ModelMetadata<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T> : DefaultModelMetadata, ITypedModelMetadata
{
    private ModelMetadata? _constructor;
    private bool _constructorInit;
    private IModelBinder? _modelBinder;

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

    public override ModelMetadata? BoundConstructor
        => !_constructorInit ? (_constructor ??= CtorInit()) : _constructor;

    public override IReadOnlyList<ModelMetadata>? BoundConstructorParameters
        => BoundConstructor?.BoundConstructorParameters;

    public override Func<object?[], object>? BoundConstructorInvoker => BoundConstructor?.BoundConstructorInvoker;

    public override ModelPropertyCollection Properties => _propertyCollection ??= new(_propertiesCache ??= PropertiesInit());

    public override IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType)
        => _propertiesCache ??= PropertiesInit();

    protected virtual ModelMetadata[] PropertiesInit() => Array.Empty<ModelMetadata>();

    protected virtual ModelMetadata? CtorInit() => null;

    protected ModelMetadata CreateMetadata(DefaultMetadataDetails details)
    {
        return _applicationModelMetadataProvider.TryCreateModelMetadata(details, _provider, _detailsProvider, _modelBindingMessageProvider, out var metadata)
            ? metadata!
            : throw new InvalidOperationException();
    }

    protected ModelMetadata? CtorInit(Type[] ctorParamsType, Func<object?[], object> ctorInvoker)
    {
        _constructorInit = true;

        // if record types
        var constructorInfo = typeof(T).GetConstructor(ctorParamsType)!;
        if (constructorInfo == null)
        {
            return null;
        }

        var constkey = ModelMetadataIdentity.ForConstructor(constructorInfo, typeof(T));
        var constrDetails = new DefaultMetadataDetails(constkey, ModelAttributes.GetAttributesForType(typeof(T)));

        var parameters = constructorInfo.GetParameters();
        var parametersMetadata = new ModelMetadata[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var key = ModelMetadataIdentity.ForParameter(parameters[i]);
            var paramDetails = new DefaultMetadataDetails(key, ModelAttributes.GetAttributesForParameter(key.ParameterInfo!, key.ModelType));
            parametersMetadata[i] = CreateMetadata(paramDetails);
        }

        constrDetails.BoundConstructorParameters = parametersMetadata;
        constrDetails.BoundConstructorInvoker = ctorInvoker;

        return new DefaultModelMetadata(_provider, _detailsProvider, constrDetails, _modelBindingMessageProvider);
    }

    public IModelBinder? CreateModelBinder(ModelBinderProviderContext context)
    {
        var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
        var mvcOptions = context.Services.GetRequiredService<IOptions<MvcOptions>>().Value;

        return CreateModelBinder(context, loggerFactory, mvcOptions);
    }

    protected virtual IModelBinder? CreateModelBinder(ModelBinderProviderContext context, ILoggerFactory loggerFactory, MvcOptions options) => null;
}