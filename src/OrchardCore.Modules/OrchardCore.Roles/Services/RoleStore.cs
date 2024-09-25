using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Documents;
using OrchardCore.Modules;
using OrchardCore.Roles.Models;
using OrchardCore.Security;

namespace OrchardCore.Roles.Services;

public class RoleStore : IRoleClaimStore<IRole>, IQueryableRoleStore<IRole>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentManager<RolesDocument> _documentManager;
    protected readonly IStringLocalizer S;
    private readonly ILogger _logger;

    private bool _updating;

    public RoleStore(
        IServiceProvider serviceProvider,
        IDocumentManager<RolesDocument> documentManager,
        IStringLocalizer<RoleStore> stringLocalizer,
        ILogger<RoleStore> logger)
    {
        _serviceProvider = serviceProvider;
        _documentManager = documentManager;
        S = stringLocalizer;
        _logger = logger;
    }

    public IQueryable<IRole> Roles => GetRolesAsync().GetAwaiter().GetResult().Roles.AsQueryable();

    /// <summary>
    /// Loads the roles document from the store for updating and that should not be cached.
    /// </summary>
    private Task<RolesDocument> LoadRolesAsync() => _documentManager.GetOrCreateMutableAsync();

    /// <summary>
    /// Gets the roles document from the cache for sharing and that should not be updated.
    /// </summary>
    private Task<RolesDocument> GetRolesAsync() => _documentManager.GetOrCreateImmutableAsync();

    /// <summary>
    /// Updates the store with the provided roles document and then updates the cache.
    /// </summary>
    private Task UpdateRolesAsync(RolesDocument roles)
    {
        _updating = true;

        return _documentManager.UpdateAsync(roles);
    }

    #region IRoleStore<IRole>

    public async Task<IdentityResult> CreateAsync(IRole role, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role);

        var roleToCreate = (Role)role;

        var roles = await LoadRolesAsync();
        roles.Roles.Add(roleToCreate);
        await UpdateRolesAsync(roles);

        var roleCreatedEventHandlers = _serviceProvider.GetRequiredService<IEnumerable<IRoleCreatedEventHandler>>();

        await roleCreatedEventHandlers.InvokeAsync((handler, roleToCreate) =>
            handler.RoleCreatedAsync(roleToCreate.RoleName), roleToCreate, _logger);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(IRole role, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role);

        var roleToRemove = (Role)role;

        if (string.Equals(roleToRemove.NormalizedRoleName, "ANONYMOUS", StringComparison.Ordinal) ||
            string.Equals(roleToRemove.NormalizedRoleName, "AUTHENTICATED", StringComparison.Ordinal))
        {
            return IdentityResult.Failed(new IdentityError { Description = S["Can't delete system roles."] });
        }

        var roleRemovedEventHandlers = _serviceProvider.GetRequiredService<IEnumerable<IRoleRemovedEventHandler>>();

        await roleRemovedEventHandlers.InvokeAsync((handler, roleToRemove) =>
            handler.RoleRemovedAsync(roleToRemove.RoleName), roleToRemove, _logger);

        var roles = await LoadRolesAsync();
        roleToRemove = roles.Roles.FirstOrDefault(r => string.Equals(r.RoleName, roleToRemove.RoleName, StringComparison.OrdinalIgnoreCase));
        roles.Roles.Remove(roleToRemove);

        await UpdateRolesAsync(roles);

        return IdentityResult.Success;
    }

    public async Task<IRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        // While updating find a role from the loaded document being mutated.
        var roles = _updating ? await LoadRolesAsync() : await GetRolesAsync();

        var role = roles.Roles.FirstOrDefault(x => string.Equals(x.RoleName, roleId, StringComparison.OrdinalIgnoreCase));

        if (role == null)
        {
            return null;
        }

        return _updating ? role : role.Clone();
    }

    public async Task<IRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        // While updating find a role from the loaded document being mutated.
        var roles = _updating ? await LoadRolesAsync() : await GetRolesAsync();

        var role = roles.Roles.FirstOrDefault(x => x.NormalizedRoleName == normalizedRoleName);

        if (role == null)
        {
            return null;
        }

        return _updating ? role : role.Clone();
    }

    public Task<string> GetNormalizedRoleNameAsync(IRole role, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role);

        return Task.FromResult(((Role)role).NormalizedRoleName);
    }

    public Task<string> GetRoleIdAsync(IRole role, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role);

        return Task.FromResult(role.RoleName.ToUpperInvariant());
    }

    public Task<string> GetRoleNameAsync(IRole role, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role);

        return Task.FromResult(role.RoleName);
    }

    public Task SetNormalizedRoleNameAsync(IRole role, string normalizedName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role);

        ((Role)role).NormalizedRoleName = normalizedName;

        return Task.CompletedTask;
    }

    public Task SetRoleNameAsync(IRole role, string roleName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role);

        ((Role)role).RoleName = roleName;

        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(IRole role, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role);

        var roles = await LoadRolesAsync();
        var existingRole = roles.Roles.FirstOrDefault(x => string.Equals(x.RoleName, role.RoleName, StringComparison.OrdinalIgnoreCase));
        roles.Roles.Remove(existingRole);
        roles.Roles.Add((Role)role);

        await UpdateRolesAsync(roles);

        return IdentityResult.Success;
    }

    #endregion IRoleStore<IRole>

    #region IRoleClaimStore<IRole>

    public Task AddClaimAsync(IRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(role);

        ArgumentNullException.ThrowIfNull(claim);

        ((Role)role).RoleClaims.Add(new RoleClaim(value: claim.Value, type: claim.Type));

        return Task.CompletedTask;
    }

    public Task<IList<Claim>> GetClaimsAsync(IRole role, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(role);

        return Task.FromResult<IList<Claim>>(((Role)role).RoleClaims.Select(x => x.ToClaim()).ToList());
    }

    public Task RemoveClaimAsync(IRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(role);

        ArgumentNullException.ThrowIfNull(claim);

        ((Role)role).RoleClaims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);

        return Task.CompletedTask;
    }

    #endregion IRoleClaimStore<IRole>

#pragma warning disable CA1816
    public void Dispose()
    {
    }
#pragma warning restore CA1816
}
