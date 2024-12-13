using Microsoft.Extensions.Options;
using OrchardCore.Contents.Indexing;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Indexing;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Settings;

namespace OrchardCore.Media.Indexing;

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

    public override async Task BuildIndexAsync(MediaField field, BuildFieldIndexContext context)
    {
        var options = context.Settings.ToOptions();
        var settings = context.ContentPartFieldDefinition.GetSettings<MediaFieldSettings>();

        if (field.Paths?.Length is null || field.Paths.Length == 0)
        {
            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key + MediaTextKeySuffix, IndexingConstants.NullValue, options);
                context.DocumentIndex.Set(key + FileTextKeySuffix, IndexingConstants.NullValue, options);
            }

            return;
        }

        if (settings.AllowMediaText)
        {
            foreach (var key in context.Keys)
            {
                if (field.MediaTexts != null)
                {
                    foreach (var mediaText in field.MediaTexts)
                    {
                        context.DocumentIndex.Set(key + MediaTextKeySuffix, mediaText, options);
                    }
                }
                else
                {
                    context.DocumentIndex.Set(key + MediaTextKeySuffix, IndexingConstants.NullValue, options);
                }
            }
        }

        var paths = new HashSet<string>();

        foreach (var path in field.Paths)
        {
            // The same file could be added several time to the field.
            if (!paths.Add(path))
            {
                // When a path is already processed, skip it.
                continue;
            }

            var providerType = _mediaFileIndexingOptions.GetRegisteredMediaFileTextProvider(Path.GetExtension(path));

            if (providerType == null)
            {
                continue;
            }

            using var fileStream = await _mediaFileStore.GetFileStreamAsync(path);

            if (fileStream == null)
            {
                continue;
            }

            var fileText = await _serviceProvider.CreateInstance<IMediaFileTextProvider>(providerType)
                .GetTextAsync(path, fileStream);

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key + FileTextKeySuffix, fileText, options);
            }
        }
    }
}
