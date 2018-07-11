using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class EnumerationFieldIndexHandler : ContentFieldIndexHandler<EnumerationField>
    {
        public override Task BuildIndexAsync(EnumerationField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(field.Value, DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
