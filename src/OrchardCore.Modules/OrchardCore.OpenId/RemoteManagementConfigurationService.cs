using System.ComponentModel.DataAnnotations;
using OpenIddict.Abstractions;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Services;

namespace OrchardCore.OpenId;

/// <summary>
/// Configures the authentication server, validation, and scope shared by remote clients.
/// Client application registration belongs to the CLI and MCP features.
/// </summary>
public sealed class RemoteManagementConfigurationService
{
    private const string ManagementScope = "orchardcore.management";
    private const string ManagementResource = "orchardcore";

    private readonly IOpenIdScopeManager _scopeManager;
    private readonly IOpenIdServerService _serverService;
    private readonly IOpenIdValidationService _validationService;
    private readonly ShellSettings _shellSettings;

    public RemoteManagementConfigurationService(
        IOpenIdScopeManager scopeManager,
        IOpenIdServerService serverService,
        IOpenIdValidationService validationService,
        ShellSettings shellSettings)
    {
        _scopeManager = scopeManager;
        _serverService = serverService;
        _validationService = validationService;
        _shellSettings = shellSettings;
    }

    /// <summary>
    /// Inspects shared authentication settings without requiring a client application.
    /// </summary>
    public async Task<RemoteManagementConfigurationStatus> GetStatusAsync()
    {
        var server = await _serverService.GetSettingsAsync();
        var validation = await _validationService.GetSettingsAsync();
        var scope = await _scopeManager.FindByNameAsync(ManagementScope);

        var scopeConfigured = false;
        if (scope is not null)
        {
            var resources = await _scopeManager.GetResourcesAsync(scope);
            scopeConfigured = resources.Contains(ManagementResource, StringComparer.Ordinal);
        }

        return new RemoteManagementConfigurationStatus
        {
            ServerEndpointsConfigured =
                server.AuthorizationEndpointPath == "/connect/authorize" &&
                server.TokenEndpointPath == "/connect/token" &&
                server.LogoutEndpointPath == "/connect/logout" &&
                server.RevocationEndpointPath == "/connect/revoke",
            AuthorizationCodeFlowConfigured = server.AllowAuthorizationCodeFlow,
            ClientCredentialsFlowConfigured = server.AllowClientCredentialsFlow,
            RefreshFlowConfigured = server.AllowRefreshTokenFlow,
            ProofKeyForCodeExchangeRequired = server.RequireProofKeyForCodeExchange,
            ValidationConfigured =
                string.Equals(validation.Tenant, _shellSettings.Name, StringComparison.Ordinal) &&
                validation.Authority is null,
            ManagementScopeConfigured = scopeConfigured,
        };
    }

    /// <summary>
    /// Repairs shared authentication settings while preserving client registrations and unrelated grants.
    /// </summary>
    public async Task ConfigureAsync()
    {
        var server = await _serverService.LoadSettingsAsync();
        server.AuthorizationEndpointPath = "/connect/authorize";
        server.TokenEndpointPath = "/connect/token";
        server.LogoutEndpointPath = "/connect/logout";
        server.RevocationEndpointPath = "/connect/revoke";
        server.AllowAuthorizationCodeFlow = true;
        server.AllowClientCredentialsFlow = true;
        server.AllowRefreshTokenFlow = true;
        server.RequireProofKeyForCodeExchange = true;
        var validation = await _validationService.LoadSettingsAsync();
        validation.Tenant = _shellSettings.Name;
        validation.Authority = null;
        validation.MetadataAddress = null;

        await ThrowIfInvalidAsync(await _serverService.ValidateSettingsAsync(server));
        await ThrowIfInvalidAsync(await _validationService.ValidateSettingsAsync(validation));

        await _serverService.UpdateSettingsAsync(server);
        await _validationService.UpdateSettingsAsync(validation);

        await ConfigureScopeAsync();
    }

    private async Task ConfigureScopeAsync()
    {
        var scope = await _scopeManager.FindByNameAsync(ManagementScope);
        var descriptor = new OpenIdScopeDescriptor();

        if (scope is not null)
        {
            await _scopeManager.PopulateAsync(scope, descriptor);
        }

        descriptor.Name = ManagementScope;
        descriptor.DisplayName = "Orchard Core remote management";
        descriptor.Description = "Allows a client to invoke Orchard Core remote management APIs.";
        descriptor.Resources.Add(ManagementResource);

        if (scope is null)
        {
            await _scopeManager.CreateAsync(descriptor);
        }
        else
        {
            await _scopeManager.UpdateAsync(scope, descriptor);
        }
    }

    private static Task ThrowIfInvalidAsync(IEnumerable<ValidationResult> results)
    {
        var errors = results
            .Where(result => result != ValidationResult.Success)
            .Select(result => result.ErrorMessage)
            .ToArray();

        return errors.Length == 0
            ? Task.CompletedTask
            : Task.FromException(new InvalidOperationException(string.Join(" ", errors)));
    }
}
