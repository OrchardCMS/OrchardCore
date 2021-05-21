using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Setup.Core;
using OrchardCore.Setup.Services;

namespace OrchardCore.Setup
{
    /// <summary>
    /// Provides extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds tenant level services.
        /// </summary>
        public static IServiceCollection AddSetup(this IServiceCollection services)
        {
            services.AddScoped<ISetupService, SetupService>();
            services.AddTransient<IConfigureOptions<SetupOptions>, SetupOptionsProvider>();
            services.AddSingleton<ISetupUserIdGenerator, SetupUserIdGenerator>();

            return services;
        }
    }
}
