using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Widgets.Models;

namespace OrchardCore.Widgets.Handlers;

public class WidgetsListPartHandler : ContentPartHandler<WidgetsListPart>
{
    private readonly IContentItemIdGenerator _idGenerator;

    public WidgetsListPartHandler(IContentItemIdGenerator idGenerator) => _idGenerator = idGenerator;

    public override Task CloningAsync(CloneContentContext context, WidgetsListPart part)
    {
        if (context.CloneContentItem.TryGet<WidgetsListPart>(out var clonedWidgetsListPart))
        {
            foreach (var zone in part.Widgets.Keys)
            {
                foreach (var contentItem in part.Widgets[zone])
                {
                    contentItem.ContentItemId = _idGenerator.GenerateUniqueId(contentItem);
                }
            }

            clonedWidgetsListPart.Apply();
        }

        return Task.CompletedTask;
    }
}
