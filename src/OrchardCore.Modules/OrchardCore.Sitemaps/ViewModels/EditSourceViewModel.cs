using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.ViewModels;

public class EditSourceViewModel
{
    public string SitemapId { get; set; }
    public string SitemapSourceId { get; set; }
    public dynamic Editor { get; set; }

    [BindNever]
    public SitemapSource SitemapSource { get; set; }

}
