using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace OrchardCore.Flows.Models;

public class FlowPart : ContentPart
{
    [BindNever]
    [Description("The ordered widget content items in the flow.")]
    public List<ContentItem> Widgets { get; set; } = [];
}
