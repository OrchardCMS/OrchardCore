using System.Threading.Tasks;
using OrchardCore.Indexing;
using OrchardCore.Title.Model;

namespace OrchardCore.Title.Indexing
{
    public class TitlePartIndexHandler : ContentPartIndexHandler<TitlePart>
    {
        public override Task BuildIndexAsync(TitlePart part, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions() 
                | DocumentIndexOptions.Analyze
                ;

            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(part.Title, DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
