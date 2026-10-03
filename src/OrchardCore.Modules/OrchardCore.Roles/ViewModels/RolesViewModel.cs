using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Roles.ViewModels;

public class RolesViewModel
{
    public List<RoleEntry> RoleEntries { get; set; } = [];

    /// <summary>
    /// The <c>AdminList</c> shape rendering the roles in the configured layout.
    /// </summary>
    public IShape List { get; set; }
}

public class RoleEntry
{
    public string Name { get; set; }

    public string Description { get; set; }

    [BindNever]
    public bool IsSystemRole { get; set; }

    [BindNever]
    public bool IsAdminRole { get; set; }
}
