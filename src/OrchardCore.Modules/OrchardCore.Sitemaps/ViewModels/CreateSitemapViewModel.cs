using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Sitemaps.ViewModels;

public class CreateSitemapViewModel
{
    [Required]
    public string Name { get; set; }

    public string Path { get; set; }

    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the editor of the new sitemap, built by the display drivers of <see cref="CreateSitemapViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
