using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Title.Models;

public class TitlePart : ContentPart
{
    [Description("The title displayed for the content item.")]
    public string Title { get; set; }
}
