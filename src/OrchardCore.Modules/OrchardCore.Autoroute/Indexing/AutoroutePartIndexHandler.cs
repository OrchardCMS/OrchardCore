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

            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(part.Path, DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
