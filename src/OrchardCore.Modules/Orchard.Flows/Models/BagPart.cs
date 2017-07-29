using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.ContentManagement;

namespace Orchard.Flows.Models
{
    public class BagPart : ContentPart
    {
        [BindNever]
        public List<ContentItem> ContentItems { get; } = new List<ContentItem>();
    }
}
