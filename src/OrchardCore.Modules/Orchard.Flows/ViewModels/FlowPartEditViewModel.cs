using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.Flows.Models;

namespace Orchard.Flows.ViewModels
{
    public class FlowPartEditViewModel
    {
        public string[] Prefixes { get; set; } = Array.Empty<string>();
        public string[] ContentTypes { get; set; } = Array.Empty<string>();

        [BindNever]
        public FlowPart FlowPart { get; set; }

        [BindNever]
        public IUpdateModel Updater { get; set; }
    }
}
