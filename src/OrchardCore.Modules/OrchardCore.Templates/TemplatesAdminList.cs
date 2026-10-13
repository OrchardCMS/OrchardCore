namespace OrchardCore.Templates;

/// <summary>
/// The admin list of templates rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="TemplatesAdminListColumnDriver"/>.
/// </summary>
public static class TemplatesAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__Templates</c> and <c>AdminListCell__Templates__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Templates";
}
