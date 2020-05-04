using OrchardCore.Infrastructure.SafeCodeFilters;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds safe code filters
        /// </summary>
        /// <param name="builder">The <see cref="OrchardCoreBuilder"/>.</param>
        public static OrchardCoreBuilder AddSafeCodeFilters(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<ISafeCodeFilterManager, DefaultSafeCodeFilterManager>();
            });

            return builder;
        }
    }
}
