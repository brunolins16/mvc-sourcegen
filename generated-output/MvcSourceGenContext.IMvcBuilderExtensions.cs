namespace Microsoft.Extensions.DependencyInjection
{
    internal static class SourceGenMvcBuilderExtensions
    {
        public static IMvcBuilder AddMvcContext(this IMvcBuilder builder)
        {
            Mvc.SourceGen.MvcSourceGenContext mvcSourceGenContext = new();
            return builder.AddMvcContext(mvcSourceGenContext);
        }
    }
}