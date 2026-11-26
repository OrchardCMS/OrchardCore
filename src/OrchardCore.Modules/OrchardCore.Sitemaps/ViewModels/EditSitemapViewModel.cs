using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Sitemaps.ViewModels;

public class EditSitemapViewModel
{
    public string SitemapId { get; set; }

    [Required]
    public string Name { get; set; }

    public string Path { get; set; }

    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the editor of the sitemap, built by the display drivers of <see cref="EditSitemapViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
