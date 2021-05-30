using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Indexing;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Settings;
using UglyToad.PdfPig;

namespace OrchardCore.Media.Handlers
{
    public class MediaFieldIndexHandler : ContentFieldIndexHandler<MediaField>
    {
        private readonly IMediaFileStore _mediaFileStore;

        public MediaFieldIndexHandler(IMediaFileStore mediaFileStore)
        {
            _mediaFileStore = mediaFileStore;
        }

        public async override Task BuildIndexAsync(MediaField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            var settings = context.ContentPartFieldDefinition.GetSettings<MediaFieldSettings>();

            if (field.Paths.Length > 0)
            {
                if (settings.AllowMediaText)
                {
                    foreach (var key in context.Keys)
                    {
                        foreach (var mediaText in field.MediaTexts)
                        {
                            context.DocumentIndex.Set(key + ".MediaText", mediaText, options);
                        }
                    }
                }

                // It doesn't really makes sense to store file contents without analyzing them for search as well.
                var fileIndexingOptions = options | DocumentIndexOptions.Analyze;

                foreach (var path in field.Paths.Where(path => path.EndsWith(".pdf")))
                {
                    using var fileStream = await _mediaFileStore.GetFileStreamAsync(path);
                    if (fileStream != null)
                    {
                        using var document = PdfDocument.Open(fileStream);
                        foreach (var page in document.GetPages())
                        {
                            foreach (var key in context.Keys)
                            {
                                context.DocumentIndex.Set(key + ".FileText", page.Text, fileIndexingOptions);
                            }
                        }
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
        }
    }
}
