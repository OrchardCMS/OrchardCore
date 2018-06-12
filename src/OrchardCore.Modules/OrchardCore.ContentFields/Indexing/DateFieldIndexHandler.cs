using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class DateFieldIndexHandler : ContentFieldIndexHandler<DateField>
    {
        public override Task BuildIndexAsync(DateField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(field.Value, DocumentIndex.Types.DateTime, options));

            return Task.CompletedTask;
        }
    }
}
