using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Sitemaps.ViewModels;

public class CreateSitemapIndexViewModel
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Path { get; set; }

    public bool Enabled { get; set; }

    public ContainableSitemapEntryViewModel[] ContainableSitemaps { get; set; } = [];

    /// <summary>
    /// Gets or sets the editor of the sitemap index, built by the display drivers of <see cref="CreateSitemapIndexViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}

public class ContainableSitemapEntryViewModel
{
    public bool IsChecked { get; set; }
    public string SitemapId { get; set; }
    public string Name { get; set; }
}
