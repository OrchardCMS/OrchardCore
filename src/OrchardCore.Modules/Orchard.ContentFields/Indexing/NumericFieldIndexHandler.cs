using System.Threading.Tasks;
using Orchard.ContentFields.Fields;
using Orchard.Indexing;

namespace Orchard.ContentFields.Indexing
{
    public class NumericFieldIndexHandler : ContentFieldIndexHandler<NumericField>
    {
        public override Task BuildIndexAsync(NumericField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(field.Value, DocumentIndex.Types.Number, options));

            return Task.CompletedTask;
        }
    }
}
