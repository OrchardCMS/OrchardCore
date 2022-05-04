using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Indexing;
using OrchardCore.Markdown.Models;

namespace OrchardCore.Markdown.Indexing
{
    public class MarkdownBodyPartIndexHandler : ContentPartIndexHandler<MarkdownBodyPart>
    {
        private const int MaxStringLength = 32766;

        public override Task BuildIndexAsync(MarkdownBodyPart part, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions()
                | DocumentIndexOptions.Sanitize
                | DocumentIndexOptions.Analyze
                ;

            foreach (var key in context.Keys)
            {
                foreach (var chunk in part.Markdown.Chunk(MaxStringLength))
                {
                    context.DocumentIndex.Set(key, new string(chunk), options);
                }
            }

            return Task.CompletedTask;
        }
    }
}
