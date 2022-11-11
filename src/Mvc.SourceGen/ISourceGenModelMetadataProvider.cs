namespace Mvc.SourceGen;

using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics.CodeAnalysis;

public interface ISourceGenModelMetadataProvider
{
    //TODO: Add notnullwhen
    bool TryCreateModelMetadata(
        DefaultMetadataDetails entry,
        IModelMetadataProvider provider,
        ICompositeMetadataDetailsProvider detailsProvider,
        DefaultModelBindingMessageProvider modelBindingMessageProvider, out ModelMetadata? modelMetadata);
}