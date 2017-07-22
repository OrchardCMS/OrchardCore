using System.Threading.Tasks;
using Orchard.Body.Model;
using Orchard.Indexing;

namespace Orchard.Body.Indexing
{
    public class BodyPartIndexHandler : ContentPartIndexHandler<BodyPart>
    {
        public override Task BuildIndexAsync(BodyPart part, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions() 
                | DocumentIndexOptions.Sanitize 
                | DocumentIndexOptions.Analyze
                ;

            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(part.Body, DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
