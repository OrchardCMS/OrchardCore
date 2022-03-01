using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.ViewModels
{
    public class FlowPartViewModel
    {
        public FlowPart FlowPart { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [BindNever]
        public BuildPartDisplayContext BuildPartDisplayContext { get; set; }
    }
}
