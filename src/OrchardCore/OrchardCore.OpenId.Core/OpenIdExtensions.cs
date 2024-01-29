using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenIddict.Abstractions;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Services.Managers;
using OrchardCore.OpenId.YesSql.Indexes;
using OrchardCore.OpenId.YesSql.Migrations;
using OrchardCore.OpenId.YesSql.Models;
using OrchardCore.OpenId.YesSql.Resolvers;
using OrchardCore.OpenId.YesSql.Stores;
using YesSql.Indexes;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OpenIdExtensions
    {
        public static OpenIddictCoreBuilder AddOrchardMigrations(this OpenIddictCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Scoped<IDataMigration, OpenIdMigrations>());

            // Configure support for an OpenId collection.
            builder.Services.Configure<StoreCollectionOptions>(o => o.Collections.Add("OpenId"));

            return builder;
        }

        public static OpenIddictCoreBuilder UseOrchardManagers(this OpenIddictCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.ReplaceApplicationManager(typeof(OpenIdApplicationManager<>))
                   .ReplaceAuthorizationManager(typeof(OpenIdAuthorizationManager<>))
                   .ReplaceScopeManager(typeof(OpenIdScopeManager<>))
                   .ReplaceTokenManager(typeof(OpenIdTokenManager<>));

            // Register proxy delegates so that the Orchard managers can be directly
            // resolved from the DI using the non-generic, Orchard-specific interfaces.
            builder.Services.TryAddScoped(provider => (IOpenIdApplicationManager)provider.GetRequiredService<IOpenIddictApplicationManager>());
            builder.Services.TryAddScoped(provider => (IOpenIdAuthorizationManager)provider.GetRequiredService<IOpenIddictAuthorizationManager>());
            builder.Services.TryAddScoped(provider => (IOpenIdScopeManager)provider.GetRequiredService<IOpenIddictScopeManager>());
            builder.Services.TryAddScoped(provider => (IOpenIdTokenManager)provider.GetRequiredService<IOpenIddictTokenManager>());

            return builder;
        }

        public static OpenIddictCoreBuilder UseYesSql(this OpenIddictCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            // Since the YesSql stores may be used with databases performing case-insensitive or
            // culture-sensitive comparisons, ensure the additional filtering logic is enforced
            // in case case-sensitive stores were registered before this extension was called.
            builder.Configure(options => options.DisableAdditionalFiltering = false);

            builder.SetDefaultApplicationEntity<OpenIdApplication>()
                   .SetDefaultAuthorizationEntity<OpenIdAuthorization>()
                   .SetDefaultScopeEntity<OpenIdScope>()
                   .SetDefaultTokenEntity<OpenIdToken>();

            builder.ReplaceApplicationStoreResolver<OpenIdApplicationStoreResolver>()
                   .ReplaceAuthorizationStoreResolver<OpenIdAuthorizationStoreResolver>()
                   .ReplaceScopeStoreResolver<OpenIdScopeStoreResolver>()
                   .ReplaceTokenStoreResolver<OpenIdTokenStoreResolver>();

            builder.Services.TryAddSingleton<OpenIdApplicationStoreResolver.TypeResolutionCache>();
            builder.Services.TryAddSingleton<OpenIdAuthorizationStoreResolver.TypeResolutionCache>();
            builder.Services.TryAddSingleton<OpenIdScopeStoreResolver.TypeResolutionCache>();
            builder.Services.TryAddSingleton<OpenIdTokenStoreResolver.TypeResolutionCache>();

            builder.Services.TryAddScoped(typeof(OpenIdApplicationStore<>));
            builder.Services.TryAddScoped(typeof(OpenIdAuthorizationStore<>));
            builder.Services.TryAddScoped(typeof(OpenIdScopeStore<>));
            builder.Services.TryAddScoped(typeof(OpenIdTokenStore<>));

            builder.Services.TryAddEnumerable(new[]
            {
                ServiceDescriptor.Singleton<IIndexProvider, OpenIdApplicationIndexProvider>(),
                ServiceDescriptor.Singleton<IIndexProvider, OpenIdAuthorizationIndexProvider>(),
                ServiceDescriptor.Singleton<IIndexProvider, OpenIdScopeIndexProvider>(),
                ServiceDescriptor.Singleton<IIndexProvider, OpenIdTokenIndexProvider>()
            });

            return builder;
        }
    }
}
