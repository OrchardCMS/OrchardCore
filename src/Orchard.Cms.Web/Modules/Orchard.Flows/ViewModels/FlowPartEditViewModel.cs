using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.Flows.Model;

namespace Orchard.Flows.ViewModels
{
    public class FlowPartEditViewModel
    {
        public int[] Indexes { get; set; } = Array.Empty<int>();
        public string[] ContentTypes { get; set; } = Array.Empty<string>();

        [BindNever]
        public FlowPart FlowPart { get; set; }

        [BindNever]
        public IUpdateModel Updater { get; set; }
    }
}
