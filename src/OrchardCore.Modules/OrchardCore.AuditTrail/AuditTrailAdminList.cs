namespace OrchardCore.AuditTrail;

/// <summary>
/// The admin list of audit trail events rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="AuditTrailAdminListColumnProvider"/>.
/// </summary>
public static class AuditTrailAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="OrchardCore.Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__AuditTrail</c> and <c>AdminListCell__AuditTrail__{Column}</c> alternates.
    /// </summary>
    public const string Name = "AuditTrail";
}
