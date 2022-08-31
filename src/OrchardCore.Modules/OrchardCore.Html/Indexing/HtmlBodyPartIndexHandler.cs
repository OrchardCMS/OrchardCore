using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Html.Extentions;
using OrchardCore.Html.Models;
using OrchardCore.Indexing;

namespace OrchardCore.Html.Indexing
{
    public class HtmlBodyPartIndexHandler : ContentPartIndexHandler<HtmlBodyPart>
    {
        private const int MaxStringLength = 32766;
        private const int IndexOverlapLength = 100;

        public override Task BuildIndexAsync(HtmlBodyPart part, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions()
                | DocumentIndexOptions.Sanitize
                | DocumentIndexOptions.Analyze
                ;

            foreach (var key in context.Keys)
            {
                foreach (var chunk in part.Html.Chunk(MaxStringLength, IndexOverlapLength))
                {
                    context.DocumentIndex.Set(key, new string(chunk), options);
                }
            }

            return Task.CompletedTask;
        }
    }
}
