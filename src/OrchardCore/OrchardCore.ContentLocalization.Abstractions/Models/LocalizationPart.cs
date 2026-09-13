using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentLocalization.Models;

public class LocalizationPart : ContentPart, ILocalizable
{
    [Description("The identifier shared by all translations of the content item.")]
    public string LocalizationSet { get; set; }

    [Description("The content item's culture as an IETF language tag, such as en-US.")]
    public string Culture { get; set; }
}
