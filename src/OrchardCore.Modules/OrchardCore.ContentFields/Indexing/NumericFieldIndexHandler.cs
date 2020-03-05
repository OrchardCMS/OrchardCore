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
            var settings = context.ContentPartFieldDefinition.GetSettings<NumericFieldSettings>();
            var options = context.Settings.ToOptions();

            if (settings.Scale == 0)
            {
                foreach (var key in context.Keys)
                {
                    context.DocumentIndex.Set(key, (int?)field.Value, options);
                }
            }
            else
            {
                foreach (var key in context.Keys)
                {
                    context.DocumentIndex.Set(key, field.Value, options);
                }
            }

            return Task.CompletedTask;
        }
    }
}
