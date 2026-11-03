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

        var clientId = ReadOptionalString(credentials, "clientId");
        var clientSecret = ReadOptionalString(credentials, "clientSecret")
            ?? throw new CliException($"The server created the application '{clientId}' but returned no client secret, so no context was saved. Rotate the secret of the application in the tenant.");

        // The server can't return this secret again, so nothing past this point may lose it.
        string contextName;
        try
        {
            contextName = await SaveProvisionedContextAsync(
                ReadOptionalString(response, "name") ?? "tenant",
                ReadOptionalString(response, "primaryUrl") ?? ReadOptionalString(response, "url") ?? throw new CliException("The server returned no tenant URL."),
                clientId ?? throw new CliException("The server returned no client identifier."),
                clientSecret,
                cancellationToken);
        }
        catch (Exception exception)
        {
            throw new CliException(await SaveProvisioningResponseAsync(response, clientId, exception));
        }

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
            EnsureIssuerMatches(discovery, stored);
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
        TenantContextRecord? context = null;

        // Choose the name while holding the lock, for parallel runs not to save under the same one.
        await _contextStore.UpdateAsync(_configuration, async configuration =>
        {
            context = new TenantContextRecord
            {
                Name = NextStepFormatter.ChooseContextName(name, configuration.Contexts),
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
            configuration.Contexts.Add(context);
            configuration.CurrentContext ??= context.Name;
        }, cancellationToken);

        return context!.Name;
    }

    // Saves the whole response in a new private file, and returns the message explaining the failure.
    private async Task<string> SaveProvisioningResponseAsync(JsonElement response, string? clientId, Exception failure)
    {
        var message = $"The server created the application '{clientId}', but no context was saved for it: {failure.Message}";
        try
        {
            _paths.EnsureRootDirectory();
            var path = Path.Combine(_paths.RootDirectory, $"provisioned-application-{Guid.NewGuid():n}.json");
            await using var file = SecretOutputFile.Create(new FileInfo(path));
            await file.WriteAsync(response, CancellationToken.None);
            return $"{message} Its credentials are in the private file '{path}', as the server can't return them again. Add a context for the tenant, supply them with OC_CLIENT_ID and OC_CLIENT_SECRET, and delete the file.";
        }
        catch (Exception exception) when (exception is CliException or IOException or UnauthorizedAccessException)
        {
            return $"{message} Its client secret couldn't be saved either ({exception.Message}), so rotate the secret of the application in the tenant.";
        }
    }

    private static string? ReadOptionalString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString())
            ? value.GetString()
            : null;
}
