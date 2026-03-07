namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    /// <summary>
    /// Adds tenant level services for managing liquid view template files.
    /// </summary>
    [Obsolete("This class is deprecated and will be removed in the upcoming major release.")]
    public static OrchardCoreBuilder AddLiquidViews(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices(services => services.AddLiquidCoreServices());

        return builder;
    }
}
