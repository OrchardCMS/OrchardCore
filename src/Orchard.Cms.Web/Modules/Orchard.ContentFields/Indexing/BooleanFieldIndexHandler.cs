using System.Threading.Tasks;
using Orchard.ContentFields.Fields;
using Orchard.Indexing;

namespace Orchard.ContentFields.Indexing
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
