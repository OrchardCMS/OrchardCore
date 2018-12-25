using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.ViewModels
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
