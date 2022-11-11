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
            if (entry.Key.ModelType == typeof(Mvc.SourceGen.Web.Models.Todo))
                modelMetadata = new MvcSourceGenWebModelsTodoModelMetadata(this, provider, detailsProvider, entry, modelBindingMessageProvider);
            if (entry.Key.ModelType == typeof(int))
                modelMetadata = new System_Int32ModelMetadata(this, provider, detailsProvider, entry, modelBindingMessageProvider);
            if (entry.Key.ModelType == typeof(string))
                modelMetadata = new System_StringModelMetadata(this, provider, detailsProvider, entry, modelBindingMessageProvider);
            if (entry.Key.ModelType == typeof(object))
                modelMetadata = new System_ObjectModelMetadata(this, provider, detailsProvider, entry, modelBindingMessageProvider);
            return modelMetadata != null;
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
            var propertyInfo0_key = ModelMetadataIdentity.ForProperty(propertyInfo0, typeof(int), typeof(Mvc.SourceGen.Web.Models.Todo));
            var propertyInfo0_attributes = ModelAttributes.GetAttributesForProperty(typeof(Mvc.SourceGen.Web.Models.Todo), propertyInfo0, typeof(int));
            var propertyInfo0_entry = new DefaultMetadataDetails(propertyInfo0_key, propertyInfo0_attributes)
            {PropertyGetter = static (obj) => ((Mvc.SourceGen.Web.Models.Todo)obj).Id!, PropertySetter = static (obj, value) => throw new InvalidOperationException("Readonly property")};

            var propertyInfo1 = typeof(Mvc.SourceGen.Web.Models.Todo).GetProperty("Title");
            var propertyInfo1_key = ModelMetadataIdentity.ForProperty(propertyInfo1, typeof(string), typeof(Mvc.SourceGen.Web.Models.Todo));
            var propertyInfo1_attributes = ModelAttributes.GetAttributesForProperty(typeof(Mvc.SourceGen.Web.Models.Todo), propertyInfo1, typeof(string));
            var propertyInfo1_entry = new DefaultMetadataDetails(propertyInfo1_key, propertyInfo1_attributes)
            {PropertyGetter = static (obj) => ((Mvc.SourceGen.Web.Models.Todo)obj).Title!, PropertySetter = static (obj, value) => throw new InvalidOperationException("Readonly property")};

            var propertyInfo2 = typeof(Mvc.SourceGen.Web.Models.Todo).GetProperty("Description");
            var propertyInfo2_key = ModelMetadataIdentity.ForProperty(propertyInfo2, typeof(string), typeof(Mvc.SourceGen.Web.Models.Todo));
            var propertyInfo2_attributes = ModelAttributes.GetAttributesForProperty(typeof(Mvc.SourceGen.Web.Models.Todo), propertyInfo2, typeof(string));
            var propertyInfo2_entry = new DefaultMetadataDetails(propertyInfo2_key, propertyInfo2_attributes)
            {PropertyGetter = static (obj) => ((Mvc.SourceGen.Web.Models.Todo)obj).Description!, PropertySetter = static (obj, value) => throw new InvalidOperationException("Readonly property")};
            return new[]{CreateMetadata(propertyInfo0_entry), CreateMetadata(propertyInfo1_entry), CreateMetadata(propertyInfo2_entry)};
        }

        protected override ModelMetadata? CtorInit() 
            => CtorInit(
                new[]{typeof(int), typeof(string), typeof(string)}, 
                static (args) => new Mvc.SourceGen.Web.Models.Todo(Id: (int)args[0], Title: (string)args[1], Description: (string)args[2]));
    }

    // Int32
    file class System_Int32ModelMetadata : ModelMetadata<int>
    {
        public System_Int32ModelMetadata(ISourceGenModelMetadataProvider applicationModelMetadataProvider, IModelMetadataProvider provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultMetadataDetails details, DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(applicationModelMetadataProvider, provider, detailsProvider, details, modelBindingMessageProvider)
        {
        }
    }

    // String
    file class System_StringModelMetadata : ModelMetadata<string>
    {
        public System_StringModelMetadata(ISourceGenModelMetadataProvider applicationModelMetadataProvider, IModelMetadataProvider provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultMetadataDetails details, DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(applicationModelMetadataProvider, provider, detailsProvider, details, modelBindingMessageProvider)
        {
        }
    }

    // Object
    file class System_ObjectModelMetadata : ModelMetadata<object>
    {
        public System_ObjectModelMetadata(ISourceGenModelMetadataProvider applicationModelMetadataProvider, IModelMetadataProvider provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultMetadataDetails details, DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(applicationModelMetadataProvider, provider, detailsProvider, details, modelBindingMessageProvider)
        {
        }
    }
}