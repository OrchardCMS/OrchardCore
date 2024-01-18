using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.ViewModels
{
    public class FlowPartEditViewModel
    {
        // Each element in a FlowPart is projected to the Prefixes and ContentTypes arrays
        // If for instance a FlowPart has three content items, there will be three elements in both
        // Prefixes and ContentTypes. Each value in Prefixes is a Guid that represents the unique
        // HtmlFieldPrefix value of its editor.

        public string[] Prefixes { get; set; } = Array.Empty<string>();
        public string[] ContentTypes { get; set; } = Array.Empty<string>();
        public string[] ContentItems { get; set; } = Array.Empty<string>();

        [BindNever]
        public FlowPart FlowPart { get; set; }

        [IgnoreDataMember]
        [BindNever]
        public IUpdateModel Updater { get; set; }

        [BindNever]
        public IEnumerable<ContentTypeDefinition> ContainedContentTypeDefinitions { get; set; }
    }
}
