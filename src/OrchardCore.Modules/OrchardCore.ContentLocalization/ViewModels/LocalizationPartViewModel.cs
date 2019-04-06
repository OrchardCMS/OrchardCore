using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentLocalization.ViewModels
{
    public class LocalizationPartViewModel
    {
        public string LocalizationSet { get; set; }
        public string Culture { get; set; }
        
        [BindNever]
        public LocalizationPart LocalizationPart { get; set; }

        [BindNever]
        public IEnumerable<LocalizationLinksViewModel> SiteCultures { get; set; }
    }

    public class LocalizationLinksViewModel
    {
        public string ContentItemId { get; set; }
        public CultureInfo Culture { get; set; }
    }
}
