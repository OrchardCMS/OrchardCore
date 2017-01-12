using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.ContentManagement;

namespace Orchard.Flows.Models
{
    public class FlowPart : ContentPart
    {
        [BindNever]
        public List<ContentItem> Widgets { get; } = new List<ContentItem>();
    }
}
