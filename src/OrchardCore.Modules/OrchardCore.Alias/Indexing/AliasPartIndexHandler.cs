using System.Threading.Tasks;
using OrchardCore.Alias.Models;
using OrchardCore.Indexing;

namespace OrchardCore.Alias.Indexing
{
    public class AliasPartIndexHandler : ContentPartIndexHandler<AliasPart>
    {
        public override Task BuildIndexAsync(AliasPart part, BuildPartIndexContext context)
        {
            var options = DocumentIndexOptions.Keyword | DocumentIndexOptions.Store;

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, part.Alias, options);
            }

            return Task.CompletedTask;
        }
    }
}
