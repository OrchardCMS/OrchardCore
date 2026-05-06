using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.Handlers;

public class BagPartHandler : ContentPartHandler<BagPart>
{
    private readonly IContentItemIdGenerator _idGenerator;

    public BagPartHandler(IContentItemIdGenerator idGenerator) => _idGenerator = idGenerator;

    public override Task CloningAsync(CloneContentContext context, BagPart part)
    {
        if (context.CloneContentItem.TryGet<BagPart>(out var clonedBagPart))
        {
            foreach (var contentItem in clonedBagPart.ContentItems)
            {
                contentItem.ContentItemId = _idGenerator.GenerateUniqueId(contentItem);
            }

            clonedBagPart.Apply();
        }

        return Task.CompletedTask;
    }

    public override Task GetContentItemAspectAsync(ContentItemAspectContext context, BagPart part)
    {
        return context.ForAsync<ContainedContentItemsAspect>(aspect =>
        {
            aspect.Accessors.Add((jsonObject) =>
            {
                // Content.Path contains the accessor for named bag parts and typed bag parts.
                var jContent = (JsonObject)part.Content;
                return jsonObject[jContent.GetNormalizedPath()]["ContentItems"] as JsonArray;
            });

            return Task.CompletedTask;
        });
    }
}
