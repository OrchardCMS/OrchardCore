using System.Threading.Tasks;
using OrchardCore.Indexing;
using OrchardCore.Markdown.Models;

namespace OrchardCore.Markdown.Indexing
{
    public class MarkdownBodyPartIndexHandler : ContentPartIndexHandler<MarkdownBodyPart>
    {
        public override Task BuildIndexAsync(MarkdownBodyPart part, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions() | DocumentIndexOptions.Sanitize;

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, part.Markdown, options);
            }

            return Task.CompletedTask;
        }
    }
}
