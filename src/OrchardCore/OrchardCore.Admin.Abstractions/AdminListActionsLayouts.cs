namespace OrchardCore.Admin;

/// <summary>
/// Well-known layouts of the row actions of an admin list and the shape naming conventions used to discover them.
/// </summary>
/// <remarks>
/// The actions of a row (the <c>Actions</c> and <c>ActionsMenu</c> zones of a <c>SummaryAdmin</c> shape) are rendered
/// by the <c>AdminListActions</c> shape using the alternate <c>AdminListActions__{Layout}</c>
/// (e.g. <c>AdminListActions-Menu.cshtml</c>). A layout is made available in the admin settings when a shape named
/// <c>AdminListActions_Option__{Layout}</c> exists, which mirrors how layouts and content field editors are discovered.
/// </remarks>
public static class AdminListActionsLayouts
{
    /// <summary>
    /// The default layout: the action buttons followed by an "Actions" dropdown for the secondary actions.
    /// </summary>
    public const string Buttons = "Buttons";

    /// <summary>
    /// The default layout: the action buttons followed by an "Actions" dropdown for the secondary actions.
    /// </summary>
    public const string DefaultLayout = "Buttons";

    /// <summary>
    /// A compact layout: a single dropdown, opened by an ellipsis button, holding every action.
    /// </summary>
    public const string Menu = "Menu";

    /// <summary>
    /// The shape type rendering the actions of a row.
    /// </summary>
    public const string ShapeType = "AdminListActions";

    /// <summary>
    /// The prefix of the shapes that declare an available layout, e.g. <c>AdminListActions_Option__Menu</c>.
    /// </summary>
    public const string OptionShapePrefix = "AdminListActions_Option__";
}
