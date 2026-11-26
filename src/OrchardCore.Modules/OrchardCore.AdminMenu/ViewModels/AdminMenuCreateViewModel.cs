using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AdminMenu.ViewModels;

public class AdminMenuCreateViewModel
{
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the editor of the new admin menu, built by the display drivers of <see cref="AdminMenuCreateViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
