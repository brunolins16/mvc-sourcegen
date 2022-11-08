#nullable enable
namespace Mvc.SourceGen
{
    using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using System.Diagnostics.CodeAnalysis;

    internal partial class MvcSourceGenContext : ISourceGenModelMetadataProvider
    {
        public bool TryCreateModelMetadata(DefaultMetadataDetails entry, IModelMetadataProvider provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultModelBindingMessageProvider modelBindingMessageProvider, out ModelMetadata? modelMetadata)
        {
            modelMetadata = null;
            if (entry.Key.ModelType == typeof(string))
                modelMetadata = new System_StringModelMetadata(this, provider, detailsProvider, entry, modelBindingMessageProvider);
            if (entry.Key.ModelType == typeof(Mvc.SourceGen.Web.Models.Todo))
                modelMetadata = new MvcSourceGenWebModelsTodoModelMetadata(this, provider, detailsProvider, entry, modelBindingMessageProvider);
            if (entry.Key.ModelType == typeof(object))
                modelMetadata = new System_ObjectModelMetadata(this, provider, detailsProvider, entry, modelBindingMessageProvider);
            return modelMetadata != null;
        }
    }

    // String
    file class System_StringModelMetadata : ModelMetadata<string>
    {
        public System_StringModelMetadata(ISourceGenModelMetadataProvider applicationModelMetadataProvider, IModelMetadataProvider provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultMetadataDetails details, DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(applicationModelMetadataProvider, provider, detailsProvider, details, modelBindingMessageProvider)
        {
        }

        protected override ModelMetadata[] PropertiesInit()
        {
            return Array.Empty<ModelMetadata>();
        }
    }

    // Todo
    file class MvcSourceGenWebModelsTodoModelMetadata : ModelMetadata<Mvc.SourceGen.Web.Models.Todo>
    {
        public MvcSourceGenWebModelsTodoModelMetadata(ISourceGenModelMetadataProvider applicationModelMetadataProvider, IModelMetadataProvider provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultMetadataDetails details, DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(applicationModelMetadataProvider, provider, detailsProvider, details, modelBindingMessageProvider)
        {
        }

        protected override ModelMetadata[] PropertiesInit()
        {
            var propertyInfo0 = typeof(Mvc.SourceGen.Web.Models.Todo).GetProperty("Id");
            var propertyInfo0_key = ModelMetadataIdentity.ForProperty(propertyInfo0, typeof(string), typeof(Mvc.SourceGen.Web.Models.Todo));
            var propertyInfo0_attributes = ModelAttributes.GetAttributesForProperty(typeof(Mvc.SourceGen.Web.Models.Todo), propertyInfo0, typeof(string));
            var propertyInfo0_entry = new DefaultMetadataDetails(propertyInfo0_key, propertyInfo0_attributes)
            {PropertyGetter = static (obj) => ((Mvc.SourceGen.Web.Models.Todo)obj).Id!, PropertySetter = static (obj, value) => ((Mvc.SourceGen.Web.Models.Todo)obj).Id = (string?)value!};

            var propertyInfo1 = typeof(Mvc.SourceGen.Web.Models.Todo).GetProperty("Title");
            var propertyInfo1_key = ModelMetadataIdentity.ForProperty(propertyInfo1, typeof(string), typeof(Mvc.SourceGen.Web.Models.Todo));
            var propertyInfo1_attributes = ModelAttributes.GetAttributesForProperty(typeof(Mvc.SourceGen.Web.Models.Todo), propertyInfo1, typeof(string));
            var propertyInfo1_entry = new DefaultMetadataDetails(propertyInfo1_key, propertyInfo1_attributes)
            {PropertyGetter = static (obj) => ((Mvc.SourceGen.Web.Models.Todo)obj).Title!, PropertySetter = static (obj, value) => ((Mvc.SourceGen.Web.Models.Todo)obj).Title = (string)value!};
            
            return new[]{CreateMetadata(propertyInfo0_entry), CreateMetadata(propertyInfo1_entry)};
        }
    }

    // Object
    file class System_ObjectModelMetadata : ModelMetadata<object>
    {
        public System_ObjectModelMetadata(ISourceGenModelMetadataProvider applicationModelMetadataProvider, IModelMetadataProvider provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultMetadataDetails details, DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(applicationModelMetadataProvider, provider, detailsProvider, details, modelBindingMessageProvider)
        {
        }

        protected override ModelMetadata[] PropertiesInit()
        {
            return Array.Empty<ModelMetadata>();
        }
    }
}