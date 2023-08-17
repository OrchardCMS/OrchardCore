using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Indexing;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Settings;

namespace OrchardCore.Media.Indexing
{
    public class MediaFieldIndexHandler : ContentFieldIndexHandler<MediaField>
    {
        private const string MediaTextKeySuffix = ".MediaText";
        private const string FileTextKeySuffix = ".FileText";

        private readonly IMediaFileStore _mediaFileStore;
        private readonly MediaFileIndexingOptions _mediaFileIndexingOptions;
        private readonly IServiceProvider _serviceProvider;

        public MediaFieldIndexHandler(
            IMediaFileStore mediaFileStore,
            IOptions<MediaFileIndexingOptions> mediaFileIndexingOptions,
            IServiceProvider serviceProvider)
        {
            _mediaFileStore = mediaFileStore;
            _mediaFileIndexingOptions = mediaFileIndexingOptions.Value;
            _serviceProvider = serviceProvider;
        }

        public async override Task BuildIndexAsync(MediaField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            var settings = context.ContentPartFieldDefinition.GetSettings<MediaFieldSettings>();

            if (field.Paths?.Length == 0)
            {
                foreach (var key in context.Keys)
                {
                    context.DocumentIndex.Set(key + MediaTextKeySuffix, "NULL", options);
                    context.DocumentIndex.Set(key + FileTextKeySuffix, "NULL", options);
                }

                return;
            }

            if (settings.AllowMediaText)
            {
                var searchableMediaText = String.Join(' ', field.MediaTexts ?? Array.Empty<string>());

                foreach (var key in context.Keys)
                {
                    if (field.MediaTexts != null)
                    {
                        context.DocumentIndex.Set(key + MediaTextKeySuffix, searchableMediaText, options);
                    }
                    else
                    {
                        context.DocumentIndex.Set(key + MediaTextKeySuffix, "NULL", options);
                    }
                }
            }

            var stringBuilder = new StringBuilder();

            foreach (var path in field.Paths)
            {
                var providerType = _mediaFileIndexingOptions.GetRegisteredMediaFileTextProvider(Path.GetExtension(path));

                if (providerType == null)
                {
                    continue;
                }

                using var fileStream = await _mediaFileStore.GetFileStreamAsync(path);

                if (fileStream != null)
                {
                    var fileText = await _serviceProvider
                        .CreateInstance<IMediaFileTextProvider>(providerType)
                        .GetTextAsync(path, fileStream);

                    stringBuilder.AppendLine(fileText);
                }
            }

            var searchableContent = stringBuilder.ToString();

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key + FileTextKeySuffix, searchableContent, options);
            }
        }
    }
}
