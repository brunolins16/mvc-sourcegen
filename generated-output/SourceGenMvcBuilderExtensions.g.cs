namespace Microsoft.Extensions.DependencyInjection
{
    using Mvc.SourceGen;

    internal static class SourceGenMvcBuilderExtensions
    {
        public static IMvcBuilder AddSourceGeneratorProviders(this IMvcBuilder builder)
        {
            MvcSourceGenContext mvcSourceGenContext = new();
            return builder.AddSourceGeneratorProviders(mvcSourceGenContext, mvcSourceGenContext);
        }
    }
}