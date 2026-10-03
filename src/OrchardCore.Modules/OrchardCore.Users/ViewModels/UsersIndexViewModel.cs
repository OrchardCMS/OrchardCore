using OrchardCore.DisplayManagement;

namespace OrchardCore.Users.ViewModels;

public class UsersIndexViewModel
{
    public IList<UserEntry> Users { get; set; }
    public UserIndexOptions Options { get; set; } = new UserIndexOptions();
    public IShape Pager { get; set; }
    public IShape Header { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering <see cref="Users"/>, <see cref="Header"/> and <see cref="Pager"/> with the configured layout.
    /// </summary>
    public IShape List { get; set; }
}

public class UserEntry
{
    public IShape Shape { get; set; }
    public string UserId { get; set; }
}
