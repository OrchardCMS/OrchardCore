using OrchardCore.Mappings;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides an extension method for <see cref="OrchardCoreBuilder"/>.
/// </summary>
public static partial class OrchardCoreBuilderExtensions
{
    /// <summary>
    /// Adds e-mail address validator service.
    /// </summary>
    /// <param name="builder">The <see cref="OrchardCoreBuilder"/>.</param>
    public static OrchardCoreBuilder AddMappings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddTransient<IMapper, Mapper>();
        });

        return builder;
    }
}
