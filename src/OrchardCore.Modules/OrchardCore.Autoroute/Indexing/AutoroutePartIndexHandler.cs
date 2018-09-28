using System.Threading.Tasks;
using OrchardCore.Autoroute.Model;
using OrchardCore.Indexing;

namespace OrchardCore.Autoroute.Indexing
{
    public class AutoroutePartIndexHandler : ContentPartIndexHandler<AutoroutePart>
    {
        public override Task BuildIndexAsync(AutoroutePart part, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions()
                & ~DocumentIndexOptions.Sanitize
                & ~DocumentIndexOptions.Analyze
                ;

            context.DocumentIndex.Set(context.Key, part.Path, options);

            return Task.CompletedTask;
        }
    }
}
