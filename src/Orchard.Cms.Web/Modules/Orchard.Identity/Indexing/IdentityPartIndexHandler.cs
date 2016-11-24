using System.Threading.Tasks;
using Orchard.Identity.Models;
using Orchard.Indexing;

namespace Orchard.Identity.Indexing
{
    public class IdentityPartIndexHandler : ContentPartIndexHandler<IdentityPart>
    {
        public override Task BuildIndexAsync(IdentityPart part, BuildPartIndexContext context)
        {
            var options = DocumentIndexOptions.Store;

            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(part.Identifier, DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
