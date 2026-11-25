using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;
using OrchardCore.Security;

namespace OrchardCore.Roles.ViewModels;

public class CloneRoleViewModel
{
    public string Name { get; set; }

    public string RoleDescription { get; set; }

    [BindNever]
    public Role Role { get; set; }

    [BindNever]
    public RolePermissionsViewModel Permissions { get; set; }

    /// <summary>
    /// Gets or sets the editor of the clone of a role, built by the display drivers of <see cref="CloneRoleViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
