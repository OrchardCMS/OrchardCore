using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Entities;
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

            services.AddIdGeneration();
            services.AddSingleton<ISetupUserIdGenerator, SetupUserIdGenerator>();

            return services;
        }
    }
}
