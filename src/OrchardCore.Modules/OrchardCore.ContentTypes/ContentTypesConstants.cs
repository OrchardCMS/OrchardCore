namespace OrchardCore.ContentTypes;

/// <summary>
/// The names of the breadcrumbs rendered by the content types and content parts screens, and the keys of the
/// contextual data each of them carries. A module adds a node to one of these trails by registering an
/// <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class ContentTypesConstants
{
    /// <summary>
    /// The breadcrumb of the content types list. It is named after the list of that screen.
    /// </summary>
    public const string TypesList = "ContentTypes";

    /// <summary>
    /// The breadcrumb of the content type creation screen.
    /// </summary>
    public const string TypesCreate = "ContentTypesCreate";

    /// <summary>
    /// The breadcrumb of the content type edition screen. It carries <see cref="TypeNameKey"/> and
    /// <see cref="TypeDisplayNameKey"/>.
    /// </summary>
    public const string TypesEdit = "ContentTypesEdit";

    /// <summary>
    /// The breadcrumb of the screen that edits a part attached to a content type. It carries
    /// <see cref="TypeNameKey"/>, <see cref="TypeDisplayNameKey"/> and <see cref="PartDisplayNameKey"/>, and it leads
    /// back to the content type the part is attached to.
    /// </summary>
    public const string TypesEditPart = "ContentTypesEditPart";

    /// <summary>
    /// The breadcrumb of the screen that attaches parts to a content type. It carries <see cref="TypeNameKey"/> and
    /// <see cref="TypeDisplayNameKey"/>.
    /// </summary>
    public const string TypesAddParts = "ContentTypesAddParts";

    /// <summary>
    /// The breadcrumb of the screen that attaches a named part to a content type. It carries <see cref="TypeNameKey"/>
    /// and <see cref="TypeDisplayNameKey"/>.
    /// </summary>
    public const string TypesAddReusablePart = "ContentTypesAddReusablePart";

    /// <summary>
    /// The breadcrumb of the screen that adds a field. It carries <see cref="PartDisplayNameKey"/>.
    /// </summary>
    public const string TypesAddField = "ContentTypesAddField";

    /// <summary>
    /// The breadcrumb of the content parts list. It is named after the list of that screen.
    /// </summary>
    public const string PartsList = "ContentParts";

    /// <summary>
    /// The breadcrumb of the content part creation screen.
    /// </summary>
    public const string PartsCreate = "ContentPartsCreate";

    /// <summary>
    /// The breadcrumb of the content part edition screen. It carries <see cref="PartNameKey"/> and
    /// <see cref="PartDisplayNameKey"/>.
    /// </summary>
    public const string PartsEdit = "ContentPartsEdit";

    /// <summary>
    /// The breadcrumb of the field settings screen. It carries <see cref="PartNameKey"/>,
    /// <see cref="PartDisplayNameKey"/> and <see cref="FieldDisplayNameKey"/>, and it leads back to the part the field
    /// belongs to.
    /// </summary>
    public const string PartsEditField = "ContentPartsEditField";

    /// <summary>
    /// The key under which a screen passes the technical name of the content type it is about.
    /// </summary>
    public const string TypeNameKey = "TypeName";

    /// <summary>
    /// The key under which a screen passes the display name of the content type it is about.
    /// </summary>
    public const string TypeDisplayNameKey = "TypeDisplayName";

    /// <summary>
    /// The key under which a screen passes the technical name of the content part it is about.
    /// </summary>
    public const string PartNameKey = "PartName";

    /// <summary>
    /// The key under which a screen passes the display name of the content part it is about.
    /// </summary>
    public const string PartDisplayNameKey = "PartDisplayName";

    /// <summary>
    /// The key under which a screen passes the display name of the field it is about.
    /// </summary>
    public const string FieldDisplayNameKey = "FieldDisplayName";
}
