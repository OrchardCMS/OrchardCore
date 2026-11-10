using OrchardCore.Facebook.Models;

namespace OrchardCore.Facebook.ViewModels;

public class MetaConversionsApiEventTaskViewModel
{
    public string EventName { get; set; }

    public string EventSourceUrl { get; set; }

    public MetaActionSource ActionSource { get; set; }

    public string EventId { get; set; }
}
