using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.ContentTypes.ViewModels
{
    public class ListContentTypesViewModel
    {
        [BindNever]
        public IEnumerable<EditTypeViewModel> Types { get; set; }
    }
}
