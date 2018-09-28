using System.Threading.Tasks;
using OrchardCore.Indexing;
using OrchardCore.Liquid.Model;

namespace OrchardCore.Liquid.Indexing
{
    public class LiquidPartIndexHandler : ContentPartIndexHandler<LiquidPart>
    {
        public override Task BuildIndexAsync(LiquidPart part, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions()
                | DocumentIndexOptions.Sanitize
                | DocumentIndexOptions.Analyze
                ;

            context.DocumentIndex.Set(context.Key, part.Liquid, options);

            return Task.CompletedTask;
        }
    }
}
