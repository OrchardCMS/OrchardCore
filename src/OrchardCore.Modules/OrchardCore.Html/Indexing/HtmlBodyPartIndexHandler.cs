using System.Threading.Tasks;
using OrchardCore.Html.Models;
using OrchardCore.Indexing;

namespace OrchardCore.Html.Indexing
{
    public class HtmlBodyPartIndexHandler : ContentPartIndexHandler<HtmlBodyPart>
    {
        public override Task BuildIndexAsync(HtmlBodyPart part, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions() | DocumentIndexOptions.Sanitize;

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, part.Html, options);
            }

            return Task.CompletedTask;
        }
    }
}
