using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Indexing.ViewModels
{
    public class ContentIndexSettingsViewModel
    {
        public ContentIndexSettings ContentIndexSettings { get; set; }

        [BindNever]
        public string DefinitionName { get; set; }
    }
}
