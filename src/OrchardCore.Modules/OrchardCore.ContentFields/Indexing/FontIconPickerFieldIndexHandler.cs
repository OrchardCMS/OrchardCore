using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{

    public class FontIconPickerFieldIndexHandler : ContentFieldIndexHandler<FontIconPickerField>
    {
        public override Task BuildIndexAsync(FontIconPickerField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(field.IconCode, DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
