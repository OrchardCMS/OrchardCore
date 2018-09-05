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
            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(string.Join(";", field.ContentItemIds), DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
