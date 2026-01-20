namespace OrchardCore.Users.ViewModels;

public class EditUserRoleViewModel
{
    public RoleEntry[] Roles { get; set; } = [];
}

public class RoleEntry
{
    public string Role { get; set; }

    public bool IsSelected { get; set; }
}
