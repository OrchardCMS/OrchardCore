using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
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
