namespace OrchardCore.Templates;

/// <summary>
/// The admin list of templates rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="TemplatesAdminListColumnProvider"/>.
/// </summary>
public static class TemplatesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Templates</c> and <c>AdminListCell__Templates__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Templates";
}
