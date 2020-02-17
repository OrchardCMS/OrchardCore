using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class EditSitemapIndexViewModel : CreateSitemapIndexViewModel
    {
        [Required]
        public string SitemapId { get; set; }

        [BindNever]
        public SitemapIndexSource SitemapIndexSource { get; set; }
    }
}
