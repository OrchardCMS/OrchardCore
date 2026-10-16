namespace OrchardCore.Contents;

/// <summary>
/// Contextual data keys shared by the content items admin list and its column providers.
/// </summary>
public static class ContentsAdminList
{
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
