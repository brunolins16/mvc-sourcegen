namespace Mvc.SourceGen;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System;

internal class SourceGenApplcationModelProvider : IApplicationModelProvider
{
    private readonly MvcOptions _mvcOptions;
    private readonly IModelMetadataProvider _modelMetadataProvider;
    private readonly ISourceGenControllerTypeProvider _sourceGenControllerTypeProvider;

    public SourceGenApplcationModelProvider(
        IOptions<MvcOptions> mvcOptionsAccessor,
        IModelMetadataProvider modelMetadataProvider,
        ISourceGenControllerTypeProvider sourceGenControllerTypeProvider)
    {
        _mvcOptions = mvcOptionsAccessor.Value;
        _modelMetadataProvider = modelMetadataProvider;
        _sourceGenControllerTypeProvider = sourceGenControllerTypeProvider;
    }

    public int Order => -1000 - 100;

    public void OnProvidersExecuted(ApplicationModelProviderContext context)
    {
    }

    public void OnProvidersExecuting(ApplicationModelProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        foreach (var controllerType in context.ControllerTypes)
        {
            var controllerModelBuilder = new ControllerModelBuilder(_modelMetadataProvider, _mvcOptions.SuppressAsyncSuffixInActionNames)
                .WithControllerType(controllerType)
                .WithApplication(context.Result);

            if (_sourceGenControllerTypeProvider.TryGetControllerInfo(controllerType, out var controllerInfo))
            {
                controllerModelBuilder = controllerModelBuilder.WithProperties(controllerInfo.Properties);
                for (int i = 0; i < controllerInfo.Actions.Length; i++)
                {
                    controllerModelBuilder = controllerModelBuilder.WithAction(
                        controllerInfo.Actions[i].Method,
                        controllerInfo.Actions[i].MethodInvoker,
                        controllerInfo.Actions[i].MethodAwaitableInfo);
                }
            }

            context.Result.Controllers.Add(controllerModelBuilder.Build());
        }
    }
}
