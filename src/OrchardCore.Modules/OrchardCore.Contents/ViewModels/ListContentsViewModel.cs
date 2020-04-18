using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Contents.Core.Options;

namespace OrchardCore.Contents.ViewModels
{
    public class ListContentsViewModel
    {
        public ListContentsViewModel()
        {
            Options = new ContentOptions();
        }

        public int? Page { get; set; }

        public ContentOptions Options { get; set; }

        [BindNever]
        public List<dynamic> ContentItems { get; set; }

        [BindNever]
        public dynamic Pager { get; set; }
    }   
}
