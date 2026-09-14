namespace OrchardCore.Users;

/// <summary>
/// The names of the breadcrumbs rendered by the users screens. A module adds a node to one of these trails by
/// registering an <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class UsersBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the users list. It is named after the list of that screen.
    /// </summary>
    public const string List = "Users";

    /// <summary>
    /// The breadcrumb of the user creation screen.
    /// </summary>
    public const string Create = "UsersCreate";

    /// <summary>
    /// The breadcrumb of the user edition screen.
    /// </summary>
    public const string Edit = "UsersEdit";

    /// <summary>
    /// The breadcrumb of the password change screen.
    /// </summary>
    public const string EditPassword = "UsersEditPassword";

    /// <summary>
    /// The breadcrumb of the user display screen.
    /// </summary>
    public const string Display = "UsersDisplay";
}
