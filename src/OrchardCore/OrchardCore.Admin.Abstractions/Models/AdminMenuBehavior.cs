namespace OrchardCore.Admin.Models;

/// <summary>
/// Defines how the sections of the admin menu open and close.
/// </summary>
public enum AdminMenuBehavior
{
    /// <summary>
    /// Only the section holding the current page is open when a page loads, and opening a
    /// section closes the other sections at the same level. This is the default.
    /// </summary>
    Focused,

    /// <summary>
    /// Every section keeps the state the user left it in, from page to page, so that several
    /// sections can be open at once. The section holding the current page is always open.
    /// </summary>
    Persistent,
}
