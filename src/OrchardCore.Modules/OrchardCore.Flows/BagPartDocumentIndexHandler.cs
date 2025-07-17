using OrchardCore.Flows.Models;
using OrchardCore.Indexing;

namespace OrchardCore.Flows;

internal sealed class BagPartDocumentIndexHandler : ContentPartIndexHandler<BagPart>
{
    public override Task BuildIndexAsync(BagPart part, BuildPartIndexContext context)
    {
        if (!context.Settings.Included)
        {
            return Task.CompletedTask;
        }

        context.DocumentIndex.Set(context.ContentTypePartDefinition.Name, part, DocumentIndexOptions.Store, new Dictionary<string, object>
        {
            { "BagPartDefinition", context.ContentTypePartDefinition },
        });

        return Task.CompletedTask;
    }
}
