using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Contents.Workflows.ViewModels
{
    public class CreateContentTaskViewModel
    {
        public IList<SelectListItem> AvailableContentTypes { get; set; }
        public string ContentType { get; set; }
        public bool Publish { get; set; }
        public string ContentProperties { get; set; }
    }
}
