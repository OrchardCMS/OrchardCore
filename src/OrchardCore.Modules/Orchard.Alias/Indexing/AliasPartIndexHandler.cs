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

            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(part.Alias, DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
