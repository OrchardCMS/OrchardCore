using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using OrchardCore.Modules;
using OrchardCore.OpenId.EntityFrameworkCore.Services;

namespace OrchardCore.OpenId.EntityFrameworkCore
{
    public class Startup : StartupBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Startup> _logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // Note: the generic stores types are constructed in a try/catch block to ensure an
            // invalid configuration doesn't prevent the entire application from loading correctly.
            try
            {
                Type contextType = GetConfigurationNodeAsType("Modules:OrchardCore.OpenId:EntityFrameworkCore:ContextType"),
                     keyType     = GetConfigurationNodeAsType("Modules:OrchardCore.OpenId:EntityFrameworkCore:KeyType") ?? typeof(string);

                if (contextType == null)
                {
                    _logger.LogWarning("The OpenID Connect module is not correctly configured.");

                    return;
                }

                var (applicationType, authorizationType, scopeType, tokenType) = GetEntityTypes(keyType);

                services.AddOpenIddict()
                    .AddCore(builder =>
                    {
                        builder.UseEntityFrameworkCore()
                               .UseDbContext(contextType);

                        builder.SetDefaultApplicationEntity(applicationType)
                               .SetDefaultAuthorizationEntity(authorizationType)
                               .SetDefaultScopeEntity(scopeType)
                               .SetDefaultTokenEntity(tokenType);
                    });

                // Remove the YesSql stores registered by the main OpenID module.
                services.RemoveAll(typeof(IOpenIddictApplicationStore<>));
                services.RemoveAll(typeof(IOpenIddictAuthorizationStore<>));
                services.RemoveAll(typeof(IOpenIddictScopeStore<>));
                services.RemoveAll(typeof(IOpenIddictTokenStore<>));

                // Override the default Entity Framework Core stores used by OpenIddict by the Orchard ones.
                services.Replace(ServiceDescriptor.Scoped(typeof(OpenIddictApplicationStore<,,,,>), typeof(OpenIdApplicationStore<,,,,>)));
                services.Replace(ServiceDescriptor.Scoped(typeof(OpenIddictAuthorizationStore<,,,,>), typeof(OpenIdAuthorizationStore<,,,,>)));
                services.Replace(ServiceDescriptor.Scoped(typeof(OpenIddictScopeStore<,,>), typeof(OpenIdScopeStore<,,>)));
                services.Replace(ServiceDescriptor.Scoped(typeof(OpenIddictTokenStore<,,,,>), typeof(OpenIdTokenStore<,,,,>)));
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "An error occurred while registering the OpenID Entity Framework Core stores.");

                return;
            }
        }

        private (Type applicationType, Type authorizationType, Type scopeType, Type tokenType) GetEntityTypes(Type keyType)
        {
            Type applicationType   = GetConfigurationNodeAsType("Modules:OrchardCore.OpenId:EntityFrameworkCore:ApplicationType"),
                 authorizationType = GetConfigurationNodeAsType("Modules:OrchardCore.OpenId:EntityFrameworkCore:AuthorizationType"),
                 scopeType         = GetConfigurationNodeAsType("Modules:OrchardCore.OpenId:EntityFrameworkCore:ScopeType"),
                 tokenType         = GetConfigurationNodeAsType("Modules:OrchardCore.OpenId:EntityFrameworkCore:TokenType");

            if (applicationType != null && authorizationType != null && scopeType != null && tokenType != null)
            {
                return (applicationType, authorizationType, scopeType, tokenType);
            }

            if (keyType == typeof(string))
            {
                return (
                    applicationType:   typeof(OpenIddictApplication),
                    authorizationType: typeof(OpenIddictAuthorization),
                    scopeType:         typeof(OpenIddictScope),
                    tokenType:         typeof(OpenIddictToken));
            }

            return (
                applicationType:   typeof(OpenIddictApplication<>).MakeGenericType(keyType),
                authorizationType: typeof(OpenIddictAuthorization<>).MakeGenericType(keyType),
                scopeType:         typeof(OpenIddictScope<>).MakeGenericType(keyType),
                tokenType:         typeof(OpenIddictToken<>).MakeGenericType(keyType));
        }

        private Type GetConfigurationNodeAsType(string node)
        {
            var name = _configuration[node];
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return Type.GetType(name, throwOnError: true, ignoreCase: true);
        }
    }
}
