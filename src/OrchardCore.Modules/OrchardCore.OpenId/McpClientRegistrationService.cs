using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OrchardCore.Locking.Distributed;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.RemoteManagement;
using ISession = YesSql.ISession;

namespace OrchardCore.OpenId;

/// <summary>Registers bounded, unprivileged MCP clients without changing server settings.</summary>
public sealed class McpClientRegistrationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOpenIdApplicationManager _applicationManager;
    private readonly IDistributedLock _distributedLock;
    private readonly ISession _session;
    private readonly IOptions<RemoteManagementMcpOptions> _options;

    /// <summary>Initializes the McpClientRegistrationService service.</summary>
    public McpClientRegistrationService(IOpenIdApplicationManager applicationManager, IDistributedLock distributedLock, ISession session, IOptions<RemoteManagementMcpOptions> options, IHttpContextAccessor httpContextAccessor)
    {
        _applicationManager = applicationManager;
        _distributedLock = distributedLock;
        _session = session;
        _options = options;
        _httpContextAccessor = httpContextAccessor;
    }

    internal const string DynamicMarker = "orchardcore:remote-management:dynamic-mcp";

    /// <summary>Creates a separate public application with explicit consent and PKCE.</summary>
    public async Task<OpenIdApplicationDescriptor> RegisterAsync(McpClientRegistrationRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);
        if (!_options.Value.AllowDynamicClientRegistration)
        {
            throw new ValidationException("Automatic MCP client registration is disabled.");
        }

        if (request.TokenEndpointAuthMethod != "none" ||
            request.GrantTypes is not { Length: > 0 } ||
            !request.GrantTypes.Contains("authorization_code", StringComparer.Ordinal) ||
            request.GrantTypes.Any(grant => grant is not ("authorization_code" or "refresh_token")) ||
            request.ResponseTypes is not ["code"] ||
            request.ClientName?.Any(char.IsControl) == true)
        {
            throw new ValidationException("Only public authorization-code clients with optional refresh tokens are supported.");
        }

        var scopes = (request.Scope ?? RemoteManagementConstants.ManagementScope)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.Ordinal).ToArray();
        if (!scopes.Contains(RemoteManagementConstants.ManagementScope, StringComparer.Ordinal) ||
            scopes.Any(scope => scope is not ("openid" or "profile" or "roles" or "offline_access" or RemoteManagementConstants.ManagementScope)))
        {
            throw new ValidationException("Request orchardcore.management, optionally with openid, profile, roles, and offline_access.");
        }

        if (request.RedirectUris.Any(uri => string.IsNullOrWhiteSpace(uri) || uri.Length > 2048 || uri.Any(char.IsWhiteSpace)))
        {
            throw new McpRedirectUriException();
        }

        Uri[] redirects;
        try
        {
            redirects = RemoteManagementMcpConfigurationService.ParseRedirectUris(string.Join('\n', request.RedirectUris));
        }
        catch (ValidationException)
        {
            throw new McpRedirectUriException();
        }

        var descriptor = new OpenIdApplicationDescriptor
        {
            ClientId = "orchardcore-mcp-" + Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16)),
            DisplayName = string.IsNullOrWhiteSpace(request.ClientName) ? "MCP client (automatically registered)" : request.ClientName,
            ApplicationType = redirects.Any(uri => uri.IsLoopback) ? OpenIddictConstants.ApplicationTypes.Native : OpenIddictConstants.ApplicationTypes.Web,
            ClientType = OpenIddictConstants.ClientTypes.Public,
            ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
        };
        var httpRequest = _httpContextAccessor.HttpContext.Request;
        var resource = new Uri($"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}/mcp").AbsoluteUri;
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Resource + resource);
        descriptor.RedirectUris.UnionWith(redirects);
        descriptor.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);
        descriptor.Permissions.UnionWith([
            OpenIddictConstants.Permissions.Endpoints.Authorization,
            OpenIddictConstants.Permissions.Endpoints.Token,
            OpenIddictConstants.Permissions.Endpoints.Revocation,
            OpenIddictConstants.Permissions.ResponseTypes.Code,
        ]);
        descriptor.Permissions.UnionWith(request.GrantTypes.Select(grant => OpenIddictConstants.Permissions.Prefixes.GrantType + grant));
        descriptor.Permissions.UnionWith(scopes.Where(scope => scope != "offline_access").Select(scope => OpenIddictConstants.Permissions.Prefixes.Scope + scope));
        descriptor.Properties["orchardcore:remote-management:client"] = JsonSerializer.SerializeToElement("mcp");
        descriptor.Properties[DynamicMarker] = JsonSerializer.SerializeToElement(true);

        // Serialize the count and commit across tenant nodes using the configured distributed lock.
        var (locker, locked) = await _distributedLock.TryAcquireLockAsync("MCP_CLIENT_REGISTRATION", TimeSpan.FromSeconds(3), TimeSpan.FromMinutes(1));
        if (!locked)
        {
            throw new McpRegistrationCapacityException();
        }

        await using (locker)
        {
            if (await _applicationManager.CountAsync(cancellationToken) >= _options.Value.MaximumApplications)
            {
                throw new McpRegistrationCapacityException();
            }

            await _applicationManager.CreateAsync(descriptor, cancellationToken);
            await _session.SaveChangesAsync(cancellationToken);
        }

        return descriptor;
    }
}

internal sealed class McpRedirectUriException : ValidationException
{
    public McpRedirectUriException() : base("Provide exact HTTPS or loopback HTTP callback URLs, without credentials, fragments, or wildcards.") { }
}

internal sealed class McpRegistrationCapacityException : Exception;

/// <summary>Bounds anonymous registration attempts per tenant and application instance.</summary>
public sealed class McpClientRegistrationLimiter : IDisposable
{
    private readonly FixedWindowRateLimiter _limiter = new(new FixedWindowRateLimiterOptions
    {
        PermitLimit = 10,
        Window = TimeSpan.FromMinutes(1),
        QueueLimit = 0,
        AutoReplenishment = true,
    });

    /// <summary>Attempts to acquire a registration permit without queuing.</summary>
    public RateLimitLease AttemptAcquire() => _limiter.AttemptAcquire();

    /// <inheritdoc />
    public void Dispose() => _limiter.Dispose();
}
