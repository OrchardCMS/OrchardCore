using System.Linq;
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

            services.Configure<AuthenticationOptions>((options) =>
            {
                if (!options.Schemes.Any(x => x.Name == "Api"))
                {
                    options.AddScheme<ApiAuthenticationHandler>("Api", null);
                }
            });

            services.AddScoped<IAuthorizationHandler, SuperUserHandler>();
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();

            return services;
        }
    }
}
