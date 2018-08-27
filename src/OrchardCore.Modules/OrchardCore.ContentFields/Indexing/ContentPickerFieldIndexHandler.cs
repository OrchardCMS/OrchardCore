using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class ContentPickerFieldIndexHandler : ContentFieldIndexHandler<ContentPickerField>
    {
        public override Task BuildIndexAsync(ContentPickerField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            // TODO: Determine how to serialize the array for Lucene
            //context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(field.ContentItemIds, DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
