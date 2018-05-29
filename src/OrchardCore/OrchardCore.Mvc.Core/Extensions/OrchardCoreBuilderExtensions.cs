using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level MVC services and configuration.
        /// </summary>
        public static OrchardCoreBuilder AddMvc(this OrchardCoreBuilder builder)
        {
            return builder.Startup.ConfigureServices((tenant, sp) =>
            {
                tenant.AddMvc(sp);
            })
            .Builder.UseStaticFiles();
        }
    }
}
