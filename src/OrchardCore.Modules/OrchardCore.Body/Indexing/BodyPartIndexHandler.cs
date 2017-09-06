using System.Threading.Tasks;
using OrchardCore.Body.Model;
using OrchardCore.Indexing;

namespace OrchardCore.Body.Indexing
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
