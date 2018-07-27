using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{

    public class YoutubeVideoFieldIndexHandler : ContentFieldIndexHandler<YoutubeVideoField>
    {
        public override Task BuildIndexAsync(YoutubeVideoField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(field.EmbededAddress, DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
