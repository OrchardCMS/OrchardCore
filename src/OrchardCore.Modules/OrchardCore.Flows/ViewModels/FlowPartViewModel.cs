using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.ViewModels
{
    public class FlowPartViewModel
    {
        public FlowPart FlowPart { get; set; }

        [IgnoreDataMember]
        [BindNever]
        public BuildPartDisplayContext BuildPartDisplayContext { get; set; }
    }
}
