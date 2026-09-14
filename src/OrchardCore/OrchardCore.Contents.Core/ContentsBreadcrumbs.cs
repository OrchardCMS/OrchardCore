namespace OrchardCore.Contents;

/// <summary>
/// The names of the breadcrumbs rendered by the content management screens, and the keys of the contextual data each
/// of them carries. A module adds a node to one of these trails by registering an
/// <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class ContentsBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the content items list. It carries <see cref="ContentTypeData"/>.
    /// </summary>
    public const string List = "Contents.List";

    /// <summary>
    /// The breadcrumb of the content item creation screen. It carries <see cref="ContentItemData"/>.
    /// </summary>
    public const string Create = "Contents.Create";

    /// <summary>
    /// The breadcrumb of the content item edition screen. It carries <see cref="ContentItemData"/>.
    /// </summary>
    public const string Edit = "Contents.Edit";

    /// <summary>
    /// The key of the <c>ContentItem</c> the screen is about.
    /// </summary>
    public const string ContentItemData = "ContentItem";

    /// <summary>
    /// The key of the name of the content type the list is filtered on, when there is one.
    /// </summary>
    public const string ContentTypeData = "ContentType";
}
