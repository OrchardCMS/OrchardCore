using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.Templates.Module.Models;
using OrchardCore.Templates.Module.Settings;

namespace OrchardCore.Templates.Module.ViewModels
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
