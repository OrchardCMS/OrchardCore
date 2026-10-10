using OrchardCore.Indexing;
using OrchardCore.Liquid.Fields;

namespace OrchardCore.Liquid.Indexing;

public sealed class LiquidFieldIndexHandler : ContentFieldIndexHandler<LiquidField>
{
    public override Task BuildIndexAsync(LiquidField field, BuildFieldIndexContext context)
    {
        var options = context.Settings.ToOptions() | DocumentIndexOptions.Sanitize;

        foreach (var key in context.Keys)
        {
            context.DocumentIndex.Set(key, field.Liquid, options);
        }

        return Task.CompletedTask;
    }
}
