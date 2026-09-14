namespace OrchardCore.Contents;

/// <summary>
/// The names of the breadcrumbs rendered by the content management screens, and the keys of the contextual data each
/// of them carries. A module adds a node to one of these trails by registering an
/// <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class ContentsBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the content items list. It carries <see cref="ContentTypeKey"/>.
    /// <para>
    /// It is named after the list it belongs to, so that the name of the breadcrumb of a screen and the name of the
    /// admin list of that screen are one and the same.
    /// </para>
    /// </summary>
    public const string List = "Contents";

    /// <summary>
    /// The breadcrumb of the content item creation screen. It carries <see cref="ContentItemKey"/>.
    /// </summary>
    public const string Create = "ContentsCreate";

    /// <summary>
    /// The breadcrumb of the content item edition screen. It carries <see cref="ContentItemKey"/>.
    /// </summary>
    public const string Edit = "ContentsEdit";

    /// <summary>
    /// The key under which the screen passes the <c>ContentItem</c> it is about.
    /// </summary>
    public const string ContentItemKey = "ContentItem";

    /// <summary>
    /// The key under which the list passes the name of the content type its items are filtered by. Empty when the
    /// items of every type are listed.
    /// </summary>
    public const string ContentTypeKey = "ContentType";
}
