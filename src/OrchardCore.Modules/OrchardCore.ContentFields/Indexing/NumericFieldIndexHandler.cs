using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing;

public class NumericFieldIndexHandler : ContentFieldIndexHandler<NumericField>
{
    public override Task BuildIndexAsync(NumericField field, BuildFieldIndexContext context)
    {
        var settings = context.ContentPartFieldDefinition.GetSettings<NumericFieldSettings>();
        var options = context.Settings.ToOptions();

        var isInteger = settings.Scale == 0;

        foreach (var key in context.Keys)
        {
            if (isInteger)
            {
                context.DocumentIndex.Set(key, (int?)field.Value, options);

                continue;
            }

            context.DocumentIndex.Set(key, field.Value, options);
        }

        return Task.CompletedTask;
    }
}
