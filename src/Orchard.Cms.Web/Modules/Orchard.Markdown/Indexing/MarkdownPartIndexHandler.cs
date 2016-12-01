using System.Threading.Tasks;
using Orchard.Markdown.Model;
using Orchard.Indexing;

namespace Orchard.Markdown.Indexing
{
    public class MarkdownPartIndexHandler : ContentPartIndexHandler<MarkdownPart>
    {
        public override Task BuildIndexAsync(MarkdownPart part, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions() 
                | DocumentIndexOptions.Sanitize 
                | DocumentIndexOptions.Analyze
                ;

            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(part.Markdown, DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
