using System.Threading.Tasks;
using OrchardCore.Indexing;
using OrchardCore.Title.Models;

namespace OrchardCore.Title.Indexing
{
    public class TitlePartIndexHandler : ContentPartIndexHandler<TitlePart>
    {
        public override Task BuildIndexAsync(TitlePart part, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions();

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, part.Title, options);
            }

            return Task.CompletedTask;
        }
    }
}
