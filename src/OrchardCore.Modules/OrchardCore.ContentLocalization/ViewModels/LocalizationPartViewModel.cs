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
        public CultureInfo CultureInfo { get { return string.IsNullOrEmpty(Culture) ? null : new CultureInfo(Culture); } }
            
        [BindNever]
        public LocalizationPart LocalizationPart { get; set; }

        [BindNever]
        public IEnumerable<LocalizationLinksViewModel> SiteCultures { get; set; }


    }

    public class LocalizationLinksViewModel
    {
        public bool IsDeleted { get; set; }
        public ContentItem ContentItem { get; set; }
        public CultureInfo Culture { get; set; }
    }
}
