using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using OrchardCore.Security;
using OrchardCore.Security.AuthorizationHandlers;
using OrchardCore.Security.Permissions;

namespace Microsoft.Extensions.DependencyInjection;

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
            services.AddScoped<IPermissionService, DefaultPermissionService>();
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        });

        builder.Configure(ValidatePermissionsAsync);

        return builder;
    }

    private static async ValueTask ValidatePermissionsAsync(IApplicationBuilder builder, IEndpointRouteBuilder routeBuilder, IServiceProvider serviceProvider)
    {
        // Make sure registered permissions are valid, i.e. they must be unique.
        var permissionProviders = serviceProvider.GetServices<IPermissionProvider>();
        var permissionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        ILogger logger = null;

        foreach (var permissionProvider in permissionProviders)
        {
            var permissions = await permissionProvider.GetPermissionsAsync();

            foreach (var permission in permissions)
            {
                if(!permissionNames.Add(permission.Name))
                {
                    logger ??= serviceProvider.GetRequiredService<ILogger<IPermissionProvider>>();

                    logger.LogError("The permission '{PermissionName}' created by the permission provider '{PermissionProvider}' is already registered. Each permission must have a unique name across all modules.", permission.Name, permissionProvider.GetType());
                }
            }
        }
    }
}
