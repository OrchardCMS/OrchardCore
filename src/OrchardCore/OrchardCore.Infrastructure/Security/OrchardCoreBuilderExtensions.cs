using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Security;
using OrchardCore.Security.AuthorizationHandlers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level services.
        /// </summary>
        public static OrchardCoreBuilder AddSecurity(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthorization();

                services.Configure<AuthenticationOptions>((options) =>
                {
                    if (!options.Schemes.Any(x => x.Name == "Api"))
                    {
                        options.AddScheme<ApiAuthenticationHandler>("Api", null);
                    }
                });

                services.AddScoped<IPermissionGrantingService, DefaultPermissionGrantingService>();
                services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            });

            return builder;
        }
    }
}
