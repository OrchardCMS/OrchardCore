using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace OrchardCore.Flows.Models
{
    public class BagPart : ContentPart
    {
        [BindNever]
        public List<ContentItem> ContentItems { get; set; } = new List<ContentItem>();
    }
}
