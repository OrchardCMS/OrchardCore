using System.Threading.Tasks;
using Orchard.ContentFields.Fields;
using Orchard.Indexing;

namespace Orchard.ContentFields.Indexing
{
    public class EnumerationFieldIndexHandler : ContentFieldIndexHandler<EnumerationField>
    {
        public override Task BuildIndexAsync(EnumerationField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(field.Value, DocumentIndex.Types.Text, options));

            // TODO: EnumerationFieldIndexHandler: Split value in multiple entries ?

            return Task.CompletedTask;
        }
    }
}
