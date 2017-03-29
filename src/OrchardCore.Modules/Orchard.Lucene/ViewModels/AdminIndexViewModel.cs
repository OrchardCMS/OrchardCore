using System.Collections.Generic;

namespace Orchard.Lucene.ViewModels
{
    public class AdminIndexViewModel
    {
        public IEnumerable<IndexViewModel> Indexes { get; set; }
    }
}
