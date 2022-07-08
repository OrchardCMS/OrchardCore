using OrchardCore.Scripting;
using OrchardCore.Scripting.Files;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides an extension method for <see cref="OrchardCoreBuilder"/>.
    /// </summary>
    public static partial class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds scripting services.
        /// </summary>
        /// <param name="builder">The <see cref="OrchardCoreBuilder"/>.</param>
        public static OrchardCoreBuilder AddScripting(this OrchardCoreBuilder builder)
        {
            builder.ApplicationServices.AddSingleton<IGlobalMethodProvider, CommonGeneratorMethods>();
            builder.ApplicationServices.AddSingleton<IScriptingEngine, FilesScriptEngine>();

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IScriptingManager, DefaultScriptingManager>();
            });

            return builder;
        }
    }
}
