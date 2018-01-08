using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.OpenId.Abstractions.Stores;
using OrchardCore.OpenId.EntityFrameworkCore.Models;
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
                Type contextType = GetContextType(), keyType = GetKeyType();
                if (contextType == null || keyType == null)
                {
                    _logger.LogWarning("The OpenID Connect module is not correctly configured.");

                    return;
                }

                var (applicationType, authorizationType, scopeType, tokenType) = GetEntityTypes(keyType);

                services.Replace(ServiceDescriptor.Scoped(
                    typeof(IOpenIdApplicationStore),
                    typeof(OpenIdApplicationStore<,,,,>).MakeGenericType(
                        applicationType, authorizationType,
                        tokenType, contextType, keyType)));

                services.Replace(ServiceDescriptor.Scoped(
                    typeof(IOpenIdAuthorizationStore),
                    typeof(OpenIdAuthorizationStore<,,,,>).MakeGenericType(
                        authorizationType, applicationType,
                        tokenType, contextType, keyType)));

                services.Replace(ServiceDescriptor.Scoped(
                    typeof(IOpenIdScopeStore),
                    typeof(OpenIdScopeStore<,,>).MakeGenericType(
                        scopeType, contextType, keyType)));

                services.Replace(ServiceDescriptor.Scoped(
                    typeof(IOpenIdTokenStore),
                    typeof(OpenIdTokenStore<,,,,>).MakeGenericType(
                        tokenType, applicationType,
                        authorizationType, contextType, keyType)));
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "An error occurred while registering the OpenID Entity Framework Core stores.");
            }
        }

        private (Type applicationType, Type authorizationType, Type scopeType, Type tokenType) GetEntityTypes(Type keyType)
        {
            Type GetEntityType(string node)
            {
                var name = _configuration[node];
                if (string.IsNullOrEmpty(name))
                {
                    return null;
                }

                return Type.GetType(name, throwOnError: true, ignoreCase: true);
            }

            Type applicationType   = GetEntityType("Modules:OrchardCore.OpenId:EntityFrameworkCore:ApplicationType"),
                 authorizationType = GetEntityType("Modules:OrchardCore.OpenId:EntityFrameworkCore:AuthorizationType"),
                 scopeType         = GetEntityType("Modules:OrchardCore.OpenId:EntityFrameworkCore:ScopeType"),
                 tokenType         = GetEntityType("Modules:OrchardCore.OpenId:EntityFrameworkCore:TokenType");

            if (applicationType != null && authorizationType != null && scopeType != null && tokenType != null)
            {
                return (applicationType, authorizationType, scopeType, tokenType);
            }

            return (
                applicationType:   typeof(OpenIdApplication<>).MakeGenericType(keyType),
                authorizationType: typeof(OpenIdAuthorization<>).MakeGenericType(keyType),
                scopeType:         typeof(OpenIdScope<>).MakeGenericType(keyType),
                tokenType:         typeof(OpenIdToken<>).MakeGenericType(keyType));
        }

        private Type GetContextType()
        {
            var name = _configuration["Modules:OrchardCore.OpenId:EntityFrameworkCore:ContextType"];
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return Type.GetType(name, throwOnError: true, ignoreCase: true);
        }

        private Type GetKeyType()
        {
            var name = _configuration["Modules:OrchardCore.OpenId:EntityFrameworkCore:KeyType"];
            if (string.IsNullOrEmpty(name))
            {
                return typeof(long);
            }

            return Type.GetType(name, throwOnError: true, ignoreCase: true);
        }
    }
}
