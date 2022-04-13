using System;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;

namespace Microsoft.AspNetCore.Builder
{
    public static class PoweredByOrchardCoreExtensions
    {
        private const string DefaultPoweredByValue = "Orchard Core";

        /// <summary>
        /// Configures whether use or not the Header X-Powered-By.
        /// Default value is OrchardCore.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        public static IApplicationBuilder UsePoweredByOrchardCore(this IApplicationBuilder app)
            => app.UsePoweredBy(DefaultPoweredByValue);

        /// <summary>
        /// Adds the <see cref="PoweredByMiddleware"/> to automatically set X-Powered-By HTTP header.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="value">The value will be used in X-Powered-By HTTP header.</param>
        public static IApplicationBuilder UsePoweredBy(this IApplicationBuilder app, string value)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"'{nameof(value)}' cannot be null or empty.", nameof(value));
            }

            var options = new PoweredByOptions { Value = value };

            return app.UseMiddleware<PoweredByMiddleware>(Options.Create(options));
        }
    }
}
