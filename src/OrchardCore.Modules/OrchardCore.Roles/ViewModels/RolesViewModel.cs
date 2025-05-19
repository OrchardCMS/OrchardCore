using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Roles.ViewModels;

public class RolesViewModel
{
    public List<RoleEntry> RoleEntries { get; set; } = [];
}

public class RoleEntry
{
    public string Name { get; set; }

    public string Description { get; set; }

    [BindNever]
    public bool IsSystemRole { get; set; }
}
