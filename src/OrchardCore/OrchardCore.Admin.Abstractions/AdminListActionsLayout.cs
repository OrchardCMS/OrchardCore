namespace OrchardCore.Admin;

/// <summary>
/// The supported presentations of an admin list row's actions.
/// </summary>
public enum AdminListActionsLayout
{
    /// <summary>
    /// Renders primary actions as buttons, followed by a dropdown for secondary actions.
    /// </summary>
    Buttons,

    /// <summary>
    /// Renders all actions in a single dropdown opened by an ellipsis button.
    /// </summary>
    Menu,
}
