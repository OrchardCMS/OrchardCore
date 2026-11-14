using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Roles.ViewModels;

public class CreateRoleViewModel
{
    [Required(AllowEmptyStrings = false)]
    public string RoleName { get; set; }

    public string RoleDescription { get; set; }

    /// <summary>
    /// Gets or sets the editor of a new role, built by the display drivers of <see cref="CreateRoleViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
