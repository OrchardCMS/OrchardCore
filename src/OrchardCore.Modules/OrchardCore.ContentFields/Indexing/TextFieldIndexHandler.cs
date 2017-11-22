using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class TextFieldIndexHandler : ContentFieldIndexHandler<TextField>
    {
        public override Task BuildIndexAsync(TextField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(field.Text, DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
