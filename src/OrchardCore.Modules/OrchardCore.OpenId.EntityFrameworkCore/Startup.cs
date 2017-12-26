using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.OpenId.Abstractions.Stores;
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
            Type contextType = GetContextType(), keyType = GetKeyType();
            if (contextType == null || keyType == null)
            {
                _logger.LogWarning("The OpenID Connect module is not correctly configured.");

                return;
            }

            services.Replace(ServiceDescriptor.Scoped(
                typeof(IOpenIdApplicationStore),
                typeof(OpenIdApplicationStore<,>).MakeGenericType(contextType, keyType)));
            services.Replace(ServiceDescriptor.Scoped(
                typeof(IOpenIdAuthorizationStore),
                typeof(OpenIdAuthorizationStore<,>).MakeGenericType(contextType, keyType)));
            services.Replace(ServiceDescriptor.Scoped(
                typeof(IOpenIdScopeStore),
                typeof(OpenIdScopeStore<,>).MakeGenericType(contextType, keyType)));
            services.Replace(ServiceDescriptor.Scoped(
                typeof(IOpenIdTokenStore),
                typeof(OpenIdTokenStore<,>).MakeGenericType(contextType, keyType)));
        }

        private Type GetContextType()
        {
            var name = _configuration["Modules:OrchardCore.OpenId:EntityFrameworkCore:ContextType"];
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var type = Type.GetType(name, throwOnError: false, ignoreCase: true);
            if (type == null || !typeof(DbContext).IsAssignableFrom(type))
            {
                return null;
            }

            return type;
        }

        private Type GetKeyType()
        {
            var name = _configuration["Modules:OrchardCore.OpenId:EntityFrameworkCore:KeyType"];
            if (string.IsNullOrEmpty(name))
            {
                return typeof(long);
            }

            var type = Type.GetType(name, throwOnError: false, ignoreCase: true);
            if (type == null || (type != typeof(Guid) && type != typeof(int) &&
                                 type != typeof(long) && type != typeof(string)))
            {
                return null;
            }

            return type;
        }
    }
}
