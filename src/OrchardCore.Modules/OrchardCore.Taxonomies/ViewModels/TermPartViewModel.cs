using System.Collections.Generic;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Taxonomies.Models;

namespace OrchardCore.Taxonomies.ViewModels
{
    public class TermPartViewModel
    {
        public TermPart TermPart { get; set; }
        public IEnumerable<ContentItem> ContentItems { get; set; }
        public BuildDisplayContext Context { get; set; }
        public List<TermEntryViewModel> Terms { get; set; }
        public dynamic Pager { get; set; }
    }
}
