namespace Mvc.SourceGen;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

public interface ISourceGenModelMetadataProvider
{
    //TODO: Add notnullwhen
    bool TryCreateModelMetadata(
        DefaultMetadataDetails entry,
        IModelMetadataProvider provider,
        ICompositeMetadataDetailsProvider detailsProvider,
        DefaultModelBindingMessageProvider modelBindingMessageProvider, out ModelMetadata? modelMetadata);
}