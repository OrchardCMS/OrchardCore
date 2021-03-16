using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace OrchardCore.Flows.Models
{
    public class FlowPart : ContentPart
    {
        [BindNever]
        public List<ContentItem> Widgets { get; set; } = new List<ContentItem>();
    }
}
