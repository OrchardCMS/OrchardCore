using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli;

internal sealed partial class CliApplication
{
    internal async Task<JsonElement> CaptureProvisionedContextAsync(JsonElement response, CancellationToken cancellationToken)
    {
        if (response.ValueKind != JsonValueKind.Object || !response.TryGetProperty("clientCredentials", out var credentials) ||
            credentials.ValueKind != JsonValueKind.Object)
        {
            return response;
        }

        var clientId = credentials.GetProperty("clientId").GetString() ?? throw new CliException("The server returned no client identifier.");
        var clientSecret = credentials.GetProperty("clientSecret").GetString() ?? throw new CliException("The server returned no client secret.");
        var name = response.GetProperty("name").GetString() ?? "tenant";
        var url = response.TryGetProperty("primaryUrl", out var primaryUrl) ? primaryUrl.GetString() : response.GetProperty("url").GetString();
        var contextName = await SaveProvisionedContextAsync(name, url ?? throw new CliException("The server returned no tenant URL."),
            clientId, clientSecret, cancellationToken);

        var output = JsonNode.Parse(response.GetRawText())!.AsObject();
        output.Remove("clientCredentials");
        output["context"] = contextName;
        output["clientId"] = clientId;
        return JsonSerializer.SerializeToElement(output, CliJsonContext.Default.JsonObject);
    }

    private async Task<string> ResolveApplicationTokenAsync(TenantContextRecord context, StoredToken stored, CancellationToken cancellationToken)
    {
        var previousKey = GetCredentialKey(context);
        RemoteManagementManifest? bootstrap = null;
        var authority = GetAuthority(context);
        if (string.IsNullOrEmpty(stored.Issuer))
        {
            // Install can finish while the local server is stopped. Discover its final
            // authority when it first runs, before sending application credentials.
            bootstrap = await FetchBootstrapAsync(context.TenantUrl, cancellationToken);
            authority = bootstrap.Authentication.Authority ?? authority;
        }

        var discovery = await _oauthClient.GetDiscoveryAsync(authority, cancellationToken);
        if (!string.IsNullOrEmpty(stored.Issuer))
        {
            CliUtilities.EnsureIssuerMatches(discovery.Issuer, stored.Issuer);
        }

        if (stored.ExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(1))
        {
            var token = await _oauthClient.ClientCredentialsAsync(discovery.TokenEndpoint, stored.ClientId!, stored.ClientSecret!,
                RemoteManagementConstants.ManagementScope, discovery.Issuer, cancellationToken);
            token.ClientId = stored.ClientId;
            token.ClientSecret = stored.ClientSecret;
            if (bootstrap is not null)
            {
                ContextStore.AddOrUpdate(_configuration, context.Name, bootstrap, makeCurrent: false);
            }

            var key = GetCredentialKey(context);
            await _credentialStore.SaveAsync(key, token, cancellationToken);
            if (bootstrap is not null)
            {
                await _contextStore.SaveAsync(_configuration, cancellationToken);
                if (key != previousKey)
                {
                    await _credentialStore.DeleteAsync(previousKey, cancellationToken);
                }
            }

            stored = token;
        }

        return stored.AccessToken;
    }

    private async Task<string> SaveProvisionedContextAsync(string name, string url, string clientId, string clientSecret, CancellationToken cancellationToken)
    {
        var tenantUrl = CliPaths.NormalizeTenantUrl(url);
        CliUriPolicy.RequireSecureEndpoint(tenantUrl);
        var context = new TenantContextRecord
        {
            Name = NextStepFormatter.ChooseContextName(name, _configuration.Contexts),
            TenantUrl = tenantUrl,
            Authority = tenantUrl,
            ClientId = RemoteManagementConstants.CliClientId,
            AddedAt = DateTimeOffset.UtcNow,
            Scopes = [RemoteManagementConstants.ManagementScope],
            GrantTypes = ["client_credentials"],
        };

        // This also works for local installation, where the temporary setup host has stopped.
        // The first authenticated command obtains a token from the site's final URL.
        await _credentialStore.SaveAsync(GetCredentialKey(context), new StoredToken
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
        }, cancellationToken);
        _configuration.Contexts.Add(context);
        _configuration.CurrentContext ??= context.Name;
        await _contextStore.SaveAsync(_configuration, cancellationToken);
        return context.Name;
    }
}
