using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Contents.Workflows.Activities;

namespace OrchardCore.Contents.Workflows.ViewModels
{
    public abstract class ContentEventViewModel<T> where T : ContentEvent
    {
        public T Activity { get; set; }
        public IList<SelectListItem> AvailableContentTypes { get; set; }
        public IList<string> SelectedContentTypeNames { get; set; } = new List<string>();
    }
}
