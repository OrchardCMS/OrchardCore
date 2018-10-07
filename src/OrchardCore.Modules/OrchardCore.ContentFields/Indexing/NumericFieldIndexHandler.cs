using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class NumericFieldIndexHandler : ContentFieldIndexHandler<NumericField>
    {
        public override Task BuildIndexAsync(NumericField field, BuildFieldIndexContext context)
        {
            var settings = context.ContentPartFieldDefinition.Settings.ToObject<NumericFieldSettings>();
            var options = context.Settings.ToOptions();

            if (settings.Scale == 0)
            {
                context.DocumentIndex.Set(context.Key, (int?)field.Value, options);
            }
            else
            {
                context.DocumentIndex.Set(context.Key, field.Value, options);
            }

            return Task.CompletedTask;
        }
    }
}
