namespace OrchardCore.Contents;

/// <summary>
/// The admin list of content items rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="ContentsAdminListColumnProvider"/>.
/// </summary>
public static class ContentsAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="OrchardCore.Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Contents</c> and <c>AdminListCell__Contents__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Contents";

    /// <summary>
    /// The key under which the list passes the content types its items are filtered by, as a <c>string[]</c>,
    /// to the column providers. Empty when the items of every type are listed.
    /// </summary>
    public const string ContentTypesKey = "ContentTypes";

    /// <summary>
    /// The key under which the list passes the stereotypes its items are filtered by, as a <c>string[]</c>,
    /// to the column providers. Empty when the items of every stereotype are listed.
    /// </summary>
    public const string StereotypesKey = "Stereotypes";
}
