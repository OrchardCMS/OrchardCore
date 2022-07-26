using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Indexing.ViewModels
{
    public class ContentIndexSettingsViewModel
    {
        public ContentIndexSettings ContentIndexSettings { get; set; }

        [BindNever]
        public bool IsStorable { get; set; } = true;
    }
}
