using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class BooleanFieldIndexHandler : ContentFieldIndexHandler<BooleanField>
    {
        public override Task BuildIndexAsync(BooleanField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(field.Value, DocumentIndex.Types.Boolean, options));

            return Task.CompletedTask;
        }
    }
}
