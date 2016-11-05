using System.Collections.Generic;

namespace Orchard.Indexing.ViewModels
{
    public class AdminIndexViewModel
    {
        public IEnumerable<string> Providers { get; set; }
        public IEnumerable<IndexViewModel> Indexes { get; set; }
    }
}
