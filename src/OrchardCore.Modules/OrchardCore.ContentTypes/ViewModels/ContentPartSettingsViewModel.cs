using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentTypes.ViewModels
{
    public class ContentPartSettingsViewModel
    {
        public bool Attachable { get; set; }
        public bool Reusable { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }

        [BindNever]
        public ContentPartDefinition ContentPartDefinition { get; set; }
    }
}
