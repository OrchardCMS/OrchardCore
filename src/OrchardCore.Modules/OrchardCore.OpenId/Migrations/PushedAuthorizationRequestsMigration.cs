#nullable enable

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Settings;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Migrations;

public sealed class PushedAuthorizationRequestsMigration : DataMigration
{
#pragma warning disable CA1822 // Mark members as static
    public int Create()
#pragma warning restore CA1822 // Mark members as static
    {
        // Note: this migration is responsible for enabling the pushed authorization endpoint in the server settings and
        // automatically granting existing applications the right to use it they are allowed to use the authorization endpoint.

        ShellScope.AddDeferredTask(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIdApplicationManager>();
            var service = scope.ServiceProvider.GetRequiredService<ISiteService>();

            List<Exception>? exceptions = null;

            // Note: for performance reasons, the maximum number of applications that can
            // be updated by this migration is 100 per batch operation and 100K in total.
            for (var index = 0; index < 1_000; index++)
            {
                List<object> applications = [];

                // Note: the applications are deliberately buffered to ensure we don't actively update
                // application entries while the applications table is still being read at the same time.
                await foreach (var application in manager.ListAsync(100, index * 100).ConfigureAwait(false))
                {
                    applications.Add(application);
                }

                foreach (var application in applications)
                {
                    // If the pushed authorization endpoint permission was already
                    // granted (manually or automatically), there's nothing left to do.
                    if (await manager.HasPermissionAsync(application, OpenIddictConstants.Permissions.Endpoints.PushedAuthorization).ConfigureAwait(false))
                    {
                        continue;
                    }

                    // If the application wasn't granted the authorization endpoint permission,
                    // do not automatically allow it to use the pushed authorization endpoint.
                    if (!await manager.HasPermissionAsync(application, OpenIddictConstants.Permissions.Endpoints.Authorization).ConfigureAwait(false))
                    {
                        continue;
                    }

                    try
                    {
                        var descriptor = new OpenIdApplicationDescriptor();
                        await manager.PopulateAsync(descriptor, application).ConfigureAwait(false);

                        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.PushedAuthorization);
                        await manager.PopulateAsync(application, descriptor).ConfigureAwait(false);
                        await manager.UpdateAsync(application).ConfigureAwait(false);
                    }

                    catch (Exception exception)
                    {
                        exceptions ??= [];
                        exceptions.Add(exception);
                    }
                }

                if (applications.Count is < 100)
                {
                    break;
                }
            }

            // If the server feature was explicitly configured (e.g using a recipe or via the GUI), update
            // the server settings stored in the database to enable the pushed authorization endpoint.
            var container = await service.LoadSiteSettingsAsync().ConfigureAwait(false);
            if (container.Properties.TryGetPropertyValue(nameof(OpenIdServerSettings), out var node) &&
                node.ToObject<OpenIdServerSettings>(JOptions.Default) is OpenIdServerSettings settings &&
                !settings.PushedAuthorizationEndpointPath.HasValue)
            {
                settings.PushedAuthorizationEndpointPath = new PathString("/connect/par");

                container.Properties[nameof(OpenIdServerSettings)] = JObject.FromObject(settings, JOptions.Default);
                await service.UpdateSiteSettingsAsync(container).ConfigureAwait(false);
            }

            if (exceptions is { Count: > 0 })
            {
                throw new AggregateException(exceptions);
            }
        });

        return 1;
    }
}
