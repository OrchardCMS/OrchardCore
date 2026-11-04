using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace OrchardCore.Flows.Models;

public class BagPart : ContentPart
{
    [BindNever]
    [Description("The ordered content items contained in the bag.")]
    public List<ContentItem> ContentItems { get; set; } = [];
}
