using System.Threading.Tasks;
using Orchard.ContentFields.Fields;
using Orchard.Indexing;

namespace Orchard.ContentFields.Indexing
{
    public class LinkFieldIndexHandler : ContentFieldIndexHandler<LinkField>
    {
        public override Task BuildIndexAsync(LinkField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(field.Url, DocumentIndex.Types.Text, options));
            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(field.Text, DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
