using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.ViewModels;

public class UsersIndexViewModel
{
    /// <summary>
    /// The queried users on this page, before their row shapes are built.
    /// </summary>
    public IList<User> Users { get; set; }
    public UserIndexOptions Options { get; set; } = new UserIndexOptions();
    public IShape Pager { get; set; }
    public IShape Header { get; set; }

    /// <summary>
    /// The data context for rendering <see cref="Users"/>, <see cref="Header"/> and <see cref="Pager"/> with the configured layout.
    /// </summary>
    public AdminListContext List { get; set; }
}

public class UserEntry
{
    public IShape Shape { get; set; }
    public string UserId { get; set; }
}
