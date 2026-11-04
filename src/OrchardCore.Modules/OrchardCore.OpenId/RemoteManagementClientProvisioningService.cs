using OpenIddict.Abstractions;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.RemoteManagement;
using OrchardCore.Roles;

namespace OrchardCore.OpenId;

/// <summary>
/// Registers dedicated confidential applications for unattended administration.
/// </summary>
public sealed class RemoteManagementClientProvisioningService : IRemoteManagementClientProvisioningService
{
    private readonly IOpenIdApplicationManager _applicationManager;
    private readonly ISystemRoleProvider _systemRoleProvider;

    public RemoteManagementClientProvisioningService(IOpenIdApplicationManager applicationManager, ISystemRoleProvider systemRoleProvider)
    {
        _applicationManager = applicationManager;
        _systemRoleProvider = systemRoleProvider;
    }

    /// <inheritdoc />
    public async Task CreateAsync(RemoteManagementClientCredentials credentials)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        ArgumentException.ThrowIfNullOrWhiteSpace(credentials.ClientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(credentials.ClientSecret);
        if (await _applicationManager.FindByClientIdAsync(credentials.ClientId) is not null)
        {
            throw new InvalidOperationException("The provisioning client identifier is already registered. Generate new credentials.");
        }

        var descriptor = new OpenIdApplicationDescriptor
        {
            ClientId = credentials.ClientId,
            ClientSecret = credentials.ClientSecret,
            DisplayName = "Pomi unattended administration",
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
        };
        descriptor.Permissions.UnionWith([
            OpenIddictConstants.Permissions.Endpoints.Token,
            OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
            OpenIddictConstants.Permissions.Prefixes.Scope + RemoteManagementConstants.ManagementScope,
        ]);
        descriptor.Roles.Add(_systemRoleProvider.GetAdminRole().RoleName);
        await _applicationManager.CreateAsync(descriptor);
    }
}
