using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Contents.ViewModels
{
    public class ListFilterContentsViewModel
    {
        public ListFilterContentsViewModel()
        {
            Options = new ContentOptions();
        }

        public ContentOptions Options { get; set; }

    }
}
