using OrchardCore.Entities;
using OrchardCore.Entities.Scripting;
using OrchardCore.Scripting;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides an extension method for <see cref="OrchardCoreBuilder"/>.
    /// </summary>
    public static partial class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds IdGeneration services.
        /// </summary>
        /// <param name="builder">The <see cref="OrchardCoreBuilder"/>.</param>
        public static OrchardCoreBuilder AddIdGeneration(this OrchardCoreBuilder builder)
        {
            var services = builder.ApplicationServices;

            services.AddSingleton<IIdGenerator, DefaultIdGenerator>();
            services.AddSingleton<IGlobalMethodProvider, IdGeneratorMethod>();

            return builder;
        }
    }
}
