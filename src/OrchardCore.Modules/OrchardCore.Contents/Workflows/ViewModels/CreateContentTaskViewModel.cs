using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Contents.Workflows.Activities;

namespace OrchardCore.Contents.Workflows.ViewModels
{
    public class CreateContentTaskViewModel : ContentTaskViewModel<CreateContentTask>
    {
        [BindNever]
        public IList<SelectListItem> AvailableContentTypes { get; set; }

        public string ContentType { get; set; }
        public bool Publish { get; set; }
        public string ContentProperties { get; set; }
    }
}
