using System.Threading.Tasks;
using OrchardCore.Markdown.Model;
using OrchardCore.Indexing;

namespace OrchardCore.Markdown.Indexing
{
    public class MarkdownBodyPartIndexHandler : ContentPartIndexHandler<MarkdownBodyPart>
    {
        public override Task BuildIndexAsync(MarkdownBodyPart part, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions() 
                | DocumentIndexOptions.Sanitize 
                | DocumentIndexOptions.Analyze
                ;

            context.DocumentIndex.Set(context.Key, part.Markdown, options);

            return Task.CompletedTask;
        }
    }
}
