using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security.AuthorizationHandlers;

namespace OrchardCore.Security
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds tenant level services.
        /// </summary>
        public static IServiceCollection AddSecurity(this IServiceCollection services)
        {
            services.AddAuthorization();
            services.AddAuthentication().AddScheme<ApiAuthorizationOptions, ApiAuthorizationHandler>("Api", null);

            services.AddScoped<IAuthorizationHandler, SuperUserHandler>();
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();

            return services;
        }
    }
}