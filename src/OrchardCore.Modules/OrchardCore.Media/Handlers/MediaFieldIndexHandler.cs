using System.Threading.Tasks;
using OrchardCore.Indexing;
using OrchardCore.Media.Fields;

namespace OrchardCore.Media.Handlers
{
    public class MediaFieldIndexHandler : ContentFieldIndexHandler<MediaField>
    {
        public override Task BuildIndexAsync(MediaField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();

            if (field.Paths.Length > 0)
            {
                foreach (var mediaText in field.MediaTexts)
                {
                    foreach (var key in context.Keys)
                    {
                        context.DocumentIndex.Set(key, mediaText, options);
                    }
                }
            }
            else
            {
                foreach (var key in context.Keys)
                {
                    context.DocumentIndex.Set(key, "NULL", options);
                }
            }

            return Task.CompletedTask;
        }
    }
}
