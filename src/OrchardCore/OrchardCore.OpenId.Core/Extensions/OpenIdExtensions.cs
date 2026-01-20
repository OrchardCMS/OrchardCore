using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenIddict.Abstractions;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Services.Managers;
using OrchardCore.OpenId.YesSql.Indexes;
using OrchardCore.OpenId.YesSql.Migrations;
using OrchardCore.OpenId.YesSql.Models;
using OrchardCore.OpenId.YesSql.Stores;

namespace Microsoft.Extensions.DependencyInjection;

public static class OpenIdExtensions
{
    public static OpenIddictCoreBuilder AddOrchardMigrations(this OpenIddictCoreBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddDataMigration<OpenIdMigrations>();

        // Configure support for an OpenId collection.
        builder.Services.Configure<StoreCollectionOptions>(o => o.Collections.Add("OpenId"));

        return builder;
    }

    public static OpenIddictCoreBuilder UseOrchardManagers(this OpenIddictCoreBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ReplaceApplicationManager(typeof(OpenIdApplicationManager<>))
               .ReplaceAuthorizationManager(typeof(OpenIdAuthorizationManager<>))
               .ReplaceScopeManager(typeof(OpenIdScopeManager<>))
               .ReplaceTokenManager(typeof(OpenIdTokenManager<>));

        // Note: OpenIddict 7.0+ no longer registers the managers under their own type.
        // To avoid a breaking change, the typed managers are manually registered here.
        builder.Services.TryAddScoped(typeof(OpenIdApplicationManager<>));
        builder.Services.TryAddScoped(typeof(OpenIdAuthorizationManager<>));
        builder.Services.TryAddScoped(typeof(OpenIdScopeManager<>));
        builder.Services.TryAddScoped(typeof(OpenIdTokenManager<>));

        // Register proxy delegates so that the Orchard managers can be directly
        // resolved from the DI using the non-generic, Orchard-specific interfaces.
        builder.Services.TryAddScoped(static provider => (IOpenIdApplicationManager)
            provider.GetRequiredService<IOpenIddictApplicationManager>());
        builder.Services.TryAddScoped(static provider => (IOpenIdAuthorizationManager)
            provider.GetRequiredService<IOpenIddictAuthorizationManager>());
        builder.Services.TryAddScoped(static provider => (IOpenIdScopeManager)
            provider.GetRequiredService<IOpenIddictScopeManager>());
        builder.Services.TryAddScoped(static provider => (IOpenIdTokenManager)
            provider.GetRequiredService<IOpenIddictTokenManager>());

        return builder;
    }

    public static OpenIddictCoreBuilder UseYesSql(this OpenIddictCoreBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Since the YesSql stores may be used with databases performing case-insensitive or
        // culture-sensitive comparisons, ensure the additional filtering logic is enforced
        // in case case-sensitive stores were registered before this extension was called.
        builder.Configure(options => options.DisableAdditionalFiltering = false);

        builder.SetDefaultApplicationEntity<OpenIdApplication>()
               .SetDefaultAuthorizationEntity<OpenIdAuthorization>()
               .SetDefaultScopeEntity<OpenIdScope>()
               .SetDefaultTokenEntity<OpenIdToken>();

        builder.ReplaceApplicationStore(typeof(OpenIdApplicationStore<>))
               .ReplaceAuthorizationStore(typeof(OpenIdAuthorizationStore<>))
               .ReplaceScopeStore(typeof(OpenIdScopeStore<>))
               .ReplaceTokenStore(typeof(OpenIdTokenStore<>));

        builder.Services.AddIndexProvider<OpenIdApplicationIndexProvider>()
                        .AddIndexProvider<OpenIdAuthorizationIndexProvider>()
                        .AddIndexProvider<OpenIdScopeIndexProvider>()
                        .AddIndexProvider<OpenIdTokenIndexProvider>();

        return builder;
    }
}
