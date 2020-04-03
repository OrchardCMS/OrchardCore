using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Setup.Services;

namespace OrchardCore.Setup
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds tenant level services.
        /// </summary>
        public static IServiceCollection AddSetup(this IServiceCollection services)
        {
            services.AddScoped<ISetupService, SetupService>();

            return services;
        }
    }
}