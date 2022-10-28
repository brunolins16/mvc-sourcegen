namespace Microsoft.Extensions.DependencyInjection;

public static class SourceGenMvcBuilderExtensions
{
    public static IMvcBuilder AddSourceGenControllers(this IMvcBuilder builder)
    {
        return builder.ConfigureApplicationPartManager(appPartManager => {
            appPartManager.ApplicationParts.Add(new SourceGenApplicationPart());
        });
    }
}