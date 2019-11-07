using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.Templates.Cms.Module.Models;
using OrchardCore.Templates.Cms.Module.Settings;

namespace OrchardCore.Templates.Cms.Module.ViewModels
{
    public class MyTestPartViewModel
    {
        public string MySetting { get; set; }

        public bool Show { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public MyTestPart MyTestPart { get; set; }

        [BindNever]
        public MyTestPartSettings Settings { get; set; }
    }
}
