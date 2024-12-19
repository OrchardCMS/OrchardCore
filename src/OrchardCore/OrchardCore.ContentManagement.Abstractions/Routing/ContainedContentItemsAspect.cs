using System.Text.Json.Nodes;

namespace OrchardCore.ContentManagement.Routing;

public class ContainedContentItemsAspect
{
    /// <summary>
    /// Json accessors to provide a list of contained content items.
    /// </summary>
    public IList<Func<JsonObject, JsonArray>> Accessors { get; set; } = [];
}
