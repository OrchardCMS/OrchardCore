using System.Collections.Generic;

namespace OrchardCore.Lucene.ViewModels
{
    public class AdminIndexViewModel
    {
        public IEnumerable<IndexViewModel> Indexes { get; set; }
    }
}
