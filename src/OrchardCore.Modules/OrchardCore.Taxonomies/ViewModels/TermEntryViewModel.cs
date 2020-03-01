using System.Collections.Generic;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.Taxonomies.ViewModels
{
    public class TermEntryViewModel
    {
        public BuildDisplayContext BuildDisplayContext { get; set; }
        public ContentItem Term { get; set; }
        public List<TermEntryViewModel> Terms { get; set; }
    }
}
