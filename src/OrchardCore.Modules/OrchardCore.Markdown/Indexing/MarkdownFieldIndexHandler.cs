using System.Threading.Tasks;
using OrchardCore.Indexing;
using OrchardCore.Markdown.Fields;

namespace OrchardCore.Markdown.Indexing
{
    public class MarkdownFieldIndexHandler : ContentFieldIndexHandler<MarkdownField>
    {
        public override Task BuildIndexAsync(MarkdownField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, field.Markdown, options);
            }

            return Task.CompletedTask;
        }
    }
}
