using System.Threading.Tasks;
using OrchardCore.Autoroute.Models;
using OrchardCore.Indexing;

namespace OrchardCore.Autoroute.Indexing
{
    public class AutoroutePartIndexHandler : ContentPartIndexHandler<AutoroutePart>
    {
        public override Task BuildIndexAsync(AutoroutePart part, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions()
                & ~DocumentIndexOptions.Sanitize
                ;

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, part.Path, options);
            }

            return Task.CompletedTask;
        }
    }
}
