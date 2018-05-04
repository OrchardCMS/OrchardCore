using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace Microsoft.AspNetCore.Builder
{
    public static class PoweredByOrchardCoreExtensions
    {
        /// <summary>
        /// Configures wethere use or not the Header X-Powered-By.
        /// Default value is OrchardCore.
        /// </summary>
        /// <param name="modularApp">The modular application builder</param>
        /// <param name="enabled">Boolean indicating if the header should be included in the response or not</param>
        /// <returns>The modular application builder</returns>
        public static ModularApplicationBuilder UsePoweredByOrchardCore(this ModularApplicationBuilder modularApp, bool enabled)
        {
            return modularApp.Configure(app =>
            {
                var options = app.ApplicationServices.GetRequiredService<IPoweredByMiddlewareOptions>();
                options.Enabled = enabled;
            });
        }

        /// <summary>
        /// Configures wethere use or not the Header X-Powered-By and its value.
        /// Default value is OrchardCore.
        /// </summary>
        /// <param name="modularApp">The modular application builder</param>
        /// <param name="enabled">Boolean indicating if the header should be included in the response or not</param>
        /// <param name="headerValue">Header's value</param>
        /// <returns>The modular application builder</returns>
        public static ModularApplicationBuilder UsePoweredBy(this ModularApplicationBuilder modularApp, bool enabled, string headerValue)
        {
            return modularApp.Configure(app =>
            {
                var options = app.ApplicationServices.GetRequiredService<IPoweredByMiddlewareOptions>();
                options.Enabled = enabled;
                options.HeaderValue = headerValue;
            });
        }
    }
}
