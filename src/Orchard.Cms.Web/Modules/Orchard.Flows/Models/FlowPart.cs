using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.ContentManagement;

namespace Orchard.Flows.Model
{
    public class FlowPart : ContentPart
    {
        [BindNever]
        public List<ContentItem> Widgets { get; } = new List<ContentItem>();

        public int[] Indexes { get; set; } = Array.Empty<int>();
    }
}
