using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{

    public class VideoFieldIndexHandler : ContentFieldIndexHandler<VideoField>
    {
        public override Task BuildIndexAsync(VideoField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(field.Address, DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
