using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OrchardCore.OpenId.Abstractions.Managers;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace OrchardCore.OpenId.Handlers;

/// <summary>Advertises automatic MCP registration only when tenant authentication is ready.</summary>
public sealed class McpRegistrationMetadataHandler : IOpenIddictServerHandler<HandleConfigurationRequestContext>
{
    private readonly RemoteManagementConfigurationService _configuration;
    private readonly IOptions<RemoteManagementMcpOptions> _options;

    /// <summary>Initializes the McpRegistrationMetadataHandler service.</summary>
    public McpRegistrationMetadataHandler(RemoteManagementConfigurationService configuration, IOptions<RemoteManagementMcpOptions> options)
    {
        _configuration = configuration;
        _options = options;
    }

    /// <inheritdoc />
    public async ValueTask HandleAsync(HandleConfigurationRequestContext context)
    {
        if (_options.Value.AllowDynamicClientRegistration && (await _configuration.GetStatusAsync()).IsReady)
        {
            context.Metadata["registration_endpoint"] = McpClientRequestValidation.GetTenantEndpoint(context.BaseUri, "connect/mcp/register");
        }
    }
}

/// <summary>Requires S256 and the current tenant's resource for automatically registered clients.</summary>
public sealed class McpAuthorizationRequestHandler : IOpenIddictServerHandler<ValidateAuthorizationRequestContext>
{
    private readonly IOpenIdApplicationManager _applications;
    private readonly IOptionsFactory<OpenIddictServerOptions> _optionsFactory;

    /// <summary>Initializes the McpAuthorizationRequestHandler service.</summary>
    public McpAuthorizationRequestHandler(IOpenIdApplicationManager applications, IOptionsFactory<OpenIddictServerOptions> optionsFactory)
    {
        _applications = applications;
        _optionsFactory = optionsFactory;
    }

    /// <inheritdoc />
    public async ValueTask HandleAsync(ValidateAuthorizationRequestContext context)
    {
        if (!await McpClientRequestValidation.IsDynamicClientAsync(_applications, context.ClientId, context.CancellationToken))
        {
            return;
        }

        if (context.Request.CodeChallengeMethod != OpenIddictConstants.CodeChallengeMethods.Sha256)
        {
            context.Reject(OpenIddictConstants.Errors.InvalidRequest, "MCP clients must use PKCE with S256.");
        }
        else if (!McpClientRequestValidation.HasExpectedResource(context.Request, context.BaseUri))
        {
            context.Reject("invalid_target", "Request this tenant's MCP resource URL.");
        }

        if (!context.IsRejected)
        {
            McpClientRequestValidation.ConfigureRequestResource(context.Transaction, _optionsFactory);
        }
    }
}

/// <summary>Prevents a dynamic client's token request from targeting another resource.</summary>
public sealed class McpTokenRequestHandler : IOpenIddictServerHandler<ValidateTokenRequestContext>
{
    private readonly IOpenIdApplicationManager _applications;
    private readonly IOptionsFactory<OpenIddictServerOptions> _optionsFactory;

    /// <summary>Initializes the McpTokenRequestHandler service.</summary>
    public McpTokenRequestHandler(IOpenIdApplicationManager applications, IOptionsFactory<OpenIddictServerOptions> optionsFactory)
    {
        _applications = applications;
        _optionsFactory = optionsFactory;
    }

    /// <inheritdoc />
    public async ValueTask HandleAsync(ValidateTokenRequestContext context)
    {
        if (!await McpClientRequestValidation.IsDynamicClientAsync(_applications, context.ClientId, context.CancellationToken))
        {
            return;
        }

        if (!McpClientRequestValidation.HasExpectedResource(context.Request, context.BaseUri))
        {
            context.Reject("invalid_target", "Request this tenant's MCP resource URL.");
        }

        if (!context.IsRejected)
        {
            McpClientRequestValidation.ConfigureRequestResource(context.Transaction, _optionsFactory);
        }
    }
}

/// <summary>Includes the MCP resource in tokens issued to automatically registered clients.</summary>
public sealed class McpTokenResourceHandler : IOpenIddictServerHandler<ProcessSignInContext>
{
    private readonly IOpenIdApplicationManager _applications;

    /// <summary>Initializes the McpTokenResourceHandler service.</summary>
    public McpTokenResourceHandler(IOpenIdApplicationManager applications)
    {
        _applications = applications;
    }

    /// <inheritdoc />
    public async ValueTask HandleAsync(ProcessSignInContext context)
    {
        if (await McpClientRequestValidation.IsDynamicClientAsync(_applications, context.Request.ClientId, context.CancellationToken))
        {
            context.Principal.SetResources(context.Principal.GetResources().Append(McpClientRequestValidation.GetTenantEndpoint(context.BaseUri, "mcp")));
        }
    }
}

internal static class McpClientRequestValidation
{
    internal static void ConfigureRequestResource(OpenIddictServerTransaction transaction, IOptionsFactory<OpenIddictServerOptions> optionsFactory)
    {
        // Build a separate options instance for this request. Never mutate the cached tenant options
        // or disable OpenIddict's resource and application permission validators.
        var options = optionsFactory.Create(Options.DefaultName);
        options.Resources.Add(new Uri(GetTenantEndpoint(transaction.BaseUri, "mcp")));
        transaction.Options = options;
    }

    // OpenIddict may supply a tenant PathBase without a trailing slash.
    internal static string GetTenantEndpoint(Uri baseUri, string path) =>
        new Uri(new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/"), path).AbsoluteUri;

    internal static bool HasExpectedResource(OpenIddictRequest request, Uri baseUri) =>
        request.GetResources() is [var resource] && string.Equals(resource, GetTenantEndpoint(baseUri, "mcp"), StringComparison.Ordinal);

    internal static async ValueTask<bool> IsDynamicClientAsync(IOpenIdApplicationManager applications, string clientId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            return false;
        }

        var application = await applications.FindByClientIdAsync(clientId, cancellationToken);
        if (application is null)
        {
            return false;
        }

        var properties = await applications.GetPropertiesAsync(application, cancellationToken);
        return properties.TryGetValue(McpClientRegistrationService.DynamicMarker, out var marker) && marker.ValueKind == System.Text.Json.JsonValueKind.True;
    }
}
