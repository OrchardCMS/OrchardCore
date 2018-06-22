using System.Threading.Tasks;
using OrchardCore.Html.Model;
using OrchardCore.Indexing;

namespace OrchardCore.Html.Indexing
{
    public class HtmlBodyPartIndexHandler : ContentPartIndexHandler<HtmlBodyPart>
    {
        public override Task BuildIndexAsync(HtmlBodyPart part, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions() 
                | DocumentIndexOptions.Sanitize 
                | DocumentIndexOptions.Analyze
                ;

            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(part.Html, DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
