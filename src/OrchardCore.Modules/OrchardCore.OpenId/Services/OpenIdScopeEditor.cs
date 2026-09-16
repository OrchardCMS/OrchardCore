using OpenIddict.Abstractions;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;

namespace OrchardCore.OpenId.Services;

internal static class OpenIdScopeEditor
{
    public static bool ContainsCurrentTenantResource(string resources, string tenantName) =>
        resources?.Contains(OpenIdConstants.Prefixes.Tenant + tenantName, StringComparison.OrdinalIgnoreCase) == true;

    public static async Task<(object Scope, bool Changed)> SaveAsync(IOpenIdScopeManager manager, object scope,
        string name, string displayName, string description, IReadOnlyCollection<string> resources,
        CancellationToken cancellationToken = default)
    {
        var descriptor = await ReadAsync(manager, scope, cancellationToken);
        if (scope is not null && Matches(descriptor, name, displayName, description, resources))
        {
            return (scope, false);
        }

        var original = scope is null ? null : await ReadAsync(manager, scope, cancellationToken);

        descriptor.Name = name;
        descriptor.DisplayName = displayName;
        descriptor.Description = description;
        // Null preserves resources for recipe updates that omit this field. An empty
        // collection explicitly clears resources, as the admin editor already does.
        if (resources is not null)
        {
            descriptor.Resources.Clear();
            descriptor.Resources.UnionWith(resources);
        }

        if (scope is null)
        {
            return (await manager.CreateAsync(descriptor, cancellationToken), true);
        }

        try
        {
            await manager.UpdateAsync(scope, descriptor, cancellationToken);
        }
        catch (OpenIddictExceptions.ValidationException)
        {
            // OpenIddict populates the tracked entity before validating it. Restore
            // its original values so a rejected edit cannot contaminate readback.
            await manager.PopulateAsync(scope, original, CancellationToken.None);
            throw;
        }
        return (scope, true);
    }

    public static async Task<bool> MatchesAsync(IOpenIdScopeManager manager, object scope,
        string name, string displayName, string description, IReadOnlyCollection<string> resources,
        CancellationToken cancellationToken = default) =>
        scope is not null && Matches(await ReadAsync(manager, scope, cancellationToken), name, displayName, description, resources);

    private static async Task<OpenIdScopeDescriptor> ReadAsync(IOpenIdScopeManager manager, object scope, CancellationToken cancellationToken)
    {
        var descriptor = new OpenIdScopeDescriptor();
        if (scope is not null)
        {
            // Copy the stored scope into the descriptor, preserving localized values
            // and extension properties that these callers do not edit.
            await manager.PopulateAsync(descriptor, scope, cancellationToken);
        }
        return descriptor;
    }

    private static bool Matches(OpenIddictScopeDescriptor descriptor, string name, string displayName,
        string description, IReadOnlyCollection<string> resources) =>
        string.Equals(descriptor.Name, name, StringComparison.Ordinal) &&
        string.Equals(descriptor.DisplayName, displayName, StringComparison.Ordinal) &&
        string.Equals(descriptor.Description, description, StringComparison.Ordinal) &&
        (resources is null || descriptor.Resources.SetEquals(resources));
}
