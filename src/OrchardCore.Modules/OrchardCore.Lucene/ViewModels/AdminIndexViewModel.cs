using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Lucene.ViewModels
{
    public class AdminIndexViewModel
    {
        public IEnumerable<IndexViewModel> Indexes { get; set; }
        [BindNever]
        public dynamic Pager { get; set; }
    }
}
