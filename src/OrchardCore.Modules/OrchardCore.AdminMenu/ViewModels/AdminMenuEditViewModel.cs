using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AdminMenu.ViewModels;

public class AdminMenuEditViewModel
{
    public string Id { get; set; }

    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the editor of the admin menu, built by the display drivers of <see cref="AdminMenuEditViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
