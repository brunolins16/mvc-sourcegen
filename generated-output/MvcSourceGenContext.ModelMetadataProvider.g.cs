#nullable enable
namespace Mvc.SourceGen
{
    using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using System.Diagnostics.CodeAnalysis;

    internal partial class MvcSourceGenContext : Microsoft.AspNetCore.Mvc.Infrastructure.ISourceGenContext
    {
        public bool TryCreateModelMetadata(DefaultMetadataDetails entry, IModelMetadataProvider provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultModelBindingMessageProvider modelBindingMessageProvider, out ModelMetadata? modelMetadata)
        {
            modelMetadata = null;
            if (entry.Key.ModelType == typeof(TodoApp.Controllers.TodoFilterRequest))
                modelMetadata = new TodoAppControllersTodoFilterRequestModelMetadata(this, provider, detailsProvider, entry, modelBindingMessageProvider);
            if (entry.Key.ModelType == typeof(string))
                modelMetadata = new System_StringModelMetadata(this, provider, detailsProvider, entry, modelBindingMessageProvider);
            if (entry.Key.ModelType == typeof(bool))
                modelMetadata = new System_BooleanModelMetadata(this, provider, detailsProvider, entry, modelBindingMessageProvider);
            if (entry.Key.ModelType == typeof(bool?))
                modelMetadata = new System_BooleanModelMetadata(this, provider, detailsProvider, entry, modelBindingMessageProvider);
            if (entry.Key.ModelType == typeof(int))
                modelMetadata = new System_Int32ModelMetadata(this, provider, detailsProvider, entry, modelBindingMessageProvider);
            if (entry.Key.ModelType == typeof(TodoApp.Controllers.NewTodo))
                modelMetadata = new TodoAppControllersNewTodoModelMetadata(this, provider, detailsProvider, entry, modelBindingMessageProvider);
            if (entry.Key.ModelType == typeof(System.Collections.Generic.List<TodoApp.Controllers.NewTodo>))
                modelMetadata = new SystemCollectionsGenericListTodoAppControllersNewTodoModelMetadata(this, provider, detailsProvider, entry, modelBindingMessageProvider);
            return modelMetadata != null;
        }
    }

    // TodoFilterRequest
    file class TodoAppControllersTodoFilterRequestModelMetadata : Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadata<TodoApp.Controllers.TodoFilterRequest>
    {
        public TodoAppControllersTodoFilterRequestModelMetadata(Microsoft.AspNetCore.Mvc.Infrastructure.ISourceGenContext sourceGenContext, IModelMetadataProvider provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultMetadataDetails details, DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(sourceGenContext, provider, detailsProvider, details, modelBindingMessageProvider)
        {
        }

        protected override Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata[] PropertiesInit()
        {
            var propertyInfo0 = typeof(TodoApp.Controllers.TodoFilterRequest).GetProperty("Title");
            var propertyInfo0_key = ModelMetadataIdentity.ForProperty(propertyInfo0, typeof(string), typeof(TodoApp.Controllers.TodoFilterRequest));
            var propertyInfo0_attributes = ModelAttributes.GetAttributesForProperty(typeof(TodoApp.Controllers.TodoFilterRequest), propertyInfo0, typeof(string));
            var propertyInfo0_entry = new DefaultMetadataDetails(propertyInfo0_key, propertyInfo0_attributes)
            {PropertyGetter = static (obj) => ((TodoApp.Controllers.TodoFilterRequest)obj).Title!, PropertySetter = static (obj, value) => throw new InvalidOperationException("Readonly property")};
            var propertyInfo1 = typeof(TodoApp.Controllers.TodoFilterRequest).GetProperty("Completed");
            var propertyInfo1_key = ModelMetadataIdentity.ForProperty(propertyInfo1, typeof(bool?), typeof(TodoApp.Controllers.TodoFilterRequest));
            var propertyInfo1_attributes = ModelAttributes.GetAttributesForProperty(typeof(TodoApp.Controllers.TodoFilterRequest), propertyInfo1, typeof(bool?));
            var propertyInfo1_entry = new DefaultMetadataDetails(propertyInfo1_key, propertyInfo1_attributes)
            {PropertyGetter = static (obj) => ((TodoApp.Controllers.TodoFilterRequest)obj).Completed!, PropertySetter = static (obj, value) => throw new InvalidOperationException("Readonly property")};
            return new[]{CreateMetadata(propertyInfo0_entry), CreateMetadata(propertyInfo1_entry)};
        }

        protected override Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata? CtorInit() => CtorInit(new[]{typeof(string), typeof(bool?)}, static (args) => new TodoApp.Controllers.TodoFilterRequest(Title: (string?)args[0], Completed: (bool?)args[1]));
    }

    // String
    file class System_StringModelMetadata : Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadata<string>
    {
        public System_StringModelMetadata(Microsoft.AspNetCore.Mvc.Infrastructure.ISourceGenContext sourceGenContext, IModelMetadataProvider provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultMetadataDetails details, DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(sourceGenContext, provider, detailsProvider, details, modelBindingMessageProvider)
        {
        }
    }

    // Boolean
    file class System_BooleanModelMetadata : Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadata<bool>
    {
        public System_BooleanModelMetadata(Microsoft.AspNetCore.Mvc.Infrastructure.ISourceGenContext sourceGenContext, IModelMetadataProvider provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultMetadataDetails details, DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(sourceGenContext, provider, detailsProvider, details, modelBindingMessageProvider)
        {
        }

        private object? ParseOperation(Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult provider, Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingContext context)
        {
            var tempSourceString = provider.FirstValue;
            object? model = null;
            if (bool.TryParse(tempSourceString, out bool parsedValue))
            {
                model = (object)parsedValue;
                context.Result = ModelBindingResult.Success(model);
            }
            else
            {
                context.ModelState.TryAddModelError(context.ModelName, new System.FormatException(), context.ModelMetadata);
            }

            return model;
        }

        protected override Microsoft.AspNetCore.Mvc.ModelBinding.IModelBinder? CreateModelBinder(Microsoft.AspNetCore.Mvc.ModelBinding.ModelBinderProviderContext context, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, Microsoft.AspNetCore.Mvc.MvcOptions options) => new Microsoft.AspNetCore.Mvc.ModelBinding.Binders.TryParseModelBinder(ParseOperation, loggerFactory);
    }

    // Int32
    file class System_Int32ModelMetadata : Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadata<int>
    {
        public System_Int32ModelMetadata(Microsoft.AspNetCore.Mvc.Infrastructure.ISourceGenContext sourceGenContext, IModelMetadataProvider provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultMetadataDetails details, DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(sourceGenContext, provider, detailsProvider, details, modelBindingMessageProvider)
        {
        }

        private object? ParseOperation(Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult provider, Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingContext context)
        {
            var tempSourceString = provider.FirstValue;
            object? model = null;
            if (int.TryParse(tempSourceString, out int parsedValue))
            {
                model = (object)parsedValue;
                context.Result = ModelBindingResult.Success(model);
            }
            else
            {
                context.ModelState.TryAddModelError(context.ModelName, new System.FormatException(), context.ModelMetadata);
            }

            return model;
        }

        protected override Microsoft.AspNetCore.Mvc.ModelBinding.IModelBinder? CreateModelBinder(Microsoft.AspNetCore.Mvc.ModelBinding.ModelBinderProviderContext context, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, Microsoft.AspNetCore.Mvc.MvcOptions options) => new Microsoft.AspNetCore.Mvc.ModelBinding.Binders.TryParseModelBinder(ParseOperation, loggerFactory);
    }

    // NewTodo
    file class TodoAppControllersNewTodoModelMetadata : Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadata<TodoApp.Controllers.NewTodo>
    {
        public TodoAppControllersNewTodoModelMetadata(Microsoft.AspNetCore.Mvc.Infrastructure.ISourceGenContext sourceGenContext, IModelMetadataProvider provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultMetadataDetails details, DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(sourceGenContext, provider, detailsProvider, details, modelBindingMessageProvider)
        {
        }

        protected override Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata[] PropertiesInit()
        {
            var propertyInfo0 = typeof(TodoApp.Controllers.NewTodo).GetProperty("Title");
            var propertyInfo0_key = ModelMetadataIdentity.ForProperty(propertyInfo0, typeof(string), typeof(TodoApp.Controllers.NewTodo));
            var propertyInfo0_attributes = ModelAttributes.GetAttributesForProperty(typeof(TodoApp.Controllers.NewTodo), propertyInfo0, typeof(string));
            var propertyInfo0_entry = new DefaultMetadataDetails(propertyInfo0_key, propertyInfo0_attributes)
            {PropertyGetter = static (obj) => ((TodoApp.Controllers.NewTodo)obj).Title!, PropertySetter = static (obj, value) => System.Runtime.CompilerServices.Unsafe.Unbox<TodoApp.Controllers.NewTodo>(obj).Title = (string)value!};
            var propertyInfo1 = typeof(TodoApp.Controllers.NewTodo).GetProperty("Description");
            var propertyInfo1_key = ModelMetadataIdentity.ForProperty(propertyInfo1, typeof(string), typeof(TodoApp.Controllers.NewTodo));
            var propertyInfo1_attributes = ModelAttributes.GetAttributesForProperty(typeof(TodoApp.Controllers.NewTodo), propertyInfo1, typeof(string));
            var propertyInfo1_entry = new DefaultMetadataDetails(propertyInfo1_key, propertyInfo1_attributes)
            {PropertyGetter = static (obj) => ((TodoApp.Controllers.NewTodo)obj).Description!, PropertySetter = static (obj, value) => System.Runtime.CompilerServices.Unsafe.Unbox<TodoApp.Controllers.NewTodo>(obj).Description = (string?)value!};
            return new[]{CreateMetadata(propertyInfo0_entry), CreateMetadata(propertyInfo1_entry)};
        }
    }

    // List`1
    file class SystemCollectionsGenericListTodoAppControllersNewTodoModelMetadata : Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadata<System.Collections.Generic.List<TodoApp.Controllers.NewTodo>>
    {
        public SystemCollectionsGenericListTodoAppControllersNewTodoModelMetadata(Microsoft.AspNetCore.Mvc.Infrastructure.ISourceGenContext sourceGenContext, IModelMetadataProvider provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultMetadataDetails details, DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(sourceGenContext, provider, detailsProvider, details, modelBindingMessageProvider)
        {
        }

        protected override Microsoft.AspNetCore.Mvc.ModelBinding.IModelBinder? CreateModelBinder(Microsoft.AspNetCore.Mvc.ModelBinding.ModelBinderProviderContext context, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, Microsoft.AspNetCore.Mvc.MvcOptions options) => new Microsoft.AspNetCore.Mvc.ModelBinding.Binders.CollectionModelBinder<TodoApp.Controllers.NewTodo>(context.CreateBinder(context.MetadataProvider.GetMetadataForType(typeof(TodoApp.Controllers.NewTodo))), loggerFactory, true, options);
    }
}