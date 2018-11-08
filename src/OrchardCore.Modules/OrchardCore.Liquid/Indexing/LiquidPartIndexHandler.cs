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

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, part.Liquid, options);
            }

            return Task.CompletedTask;
        }
    }
}
