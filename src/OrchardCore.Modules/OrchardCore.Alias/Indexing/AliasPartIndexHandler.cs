using System.Threading.Tasks;
using OrchardCore.Alias.Models;
using OrchardCore.Indexing;

namespace OrchardCore.Alias.Indexing
{
    public class AliasPartIndexHandler : ContentPartIndexHandler<AliasPart>
    {
        public override Task BuildIndexAsync(AliasPart part, BuildPartIndexContext context)
        {
            var options = DocumentIndexOptions.Store;

            context.DocumentIndex.Set(context.Key, part.Alias, options);

            return Task.CompletedTask;
        }
    }
}
