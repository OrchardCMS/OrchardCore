using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace OrchardCore.Users.ViewModels
{
    public class CustomUserSettingsEditViewModel
    {
        public dynamic Editor { get; set; }
        [BindNever]
        public ContentItem ContentItem { get; set; }
    }
}
