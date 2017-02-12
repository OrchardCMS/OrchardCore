using System.Threading.Tasks;
using Orchard.Indexing;
using Orchard.Markdown.Fields;

namespace Orchard.Markdown.Indexing
{
    public class MarkdownFieldIndexHandler : ContentFieldIndexHandler<MarkdownField>
    {
        public override Task BuildIndexAsync(MarkdownField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(field.Markdown, DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
