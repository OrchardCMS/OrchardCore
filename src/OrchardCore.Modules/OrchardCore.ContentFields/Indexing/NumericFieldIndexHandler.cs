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
                context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry((int?)field.Value, DocumentIndex.Types.Integer, options));
            }
            else
            {
                context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(field.Value, DocumentIndex.Types.Number, options));
            }

            return Task.CompletedTask;
        }
    }
}
