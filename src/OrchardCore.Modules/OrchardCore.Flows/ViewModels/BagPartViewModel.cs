using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.ViewModels
{
    public class BagPartViewModel
    {
        public BagPart BagPart { get; set; }
        public IEnumerable<ContentItem> ContentItems => BagPart.ContentItems;

        [IgnoreDataMember]
        [BindNever]
        public BuildPartDisplayContext BuildPartDisplayContext { get; set; }

        public BagPartSettings Settings { get; set; }
        public string DisplayType => Settings?.DisplayType;
    }
}
