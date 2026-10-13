namespace OrchardCore.ContentTypes;

/// <summary>
/// The admin list of content types rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="ContentTypesAdminListColumnDriver"/>.
/// </summary>
public static class ContentTypesAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__ContentTypes</c> and <c>AdminListCell__ContentTypes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "ContentTypes";
}

/// <summary>
/// The admin list of content parts rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="ContentPartsAdminListColumnDriver"/>.
/// </summary>
public static class ContentPartsAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__ContentParts</c> and <c>AdminListCell__ContentParts__{Column}</c> alternates.
    /// </summary>
    public const string Name = "ContentParts";
}
