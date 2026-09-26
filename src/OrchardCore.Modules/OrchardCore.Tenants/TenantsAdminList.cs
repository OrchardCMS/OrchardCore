namespace OrchardCore.Tenants;

/// <summary>
/// The admin list of tenants rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="TenantsAdminListColumnProvider"/>.
/// </summary>
public static class TenantsAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Tenants</c> and <c>AdminListCell__Tenants__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Tenants";
}
