using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Media.Fields;

namespace OrchardCore.Media.GraphQL;

public class MediaFieldQueryObjectType : ObjectGraphType<MediaField>
{
    public MediaFieldQueryObjectType(IStringLocalizer<MediaFieldQueryObjectType> S)
    {
        Name = nameof(MediaField);

        if (MediaAppContextSwitches.EnableLegacyMediaFields)
        {
            Field<ListGraphType<StringGraphType>, IEnumerable<string>>("paths")
                .Description(S["the media paths"])
                .PagingArguments()
                .Resolve(x =>
                {
                    if (x.Source?.Paths is null)
                    {
                        return Array.Empty<string>();
                    }

                    return x.Page(x.Source.Paths);
                });

            Field<ListGraphType<StringGraphType>, IEnumerable<string>>("fileNames")
                .Description(S["the media file names"])
                .PagingArguments()
                .Resolve(x =>
                {
                    var fileNames = x.Page(x.Source.GetAttachedFileNames());
                    if (fileNames is null)
                    {
                        return Array.Empty<string>();
                    }
                    return fileNames;
                });

            Field<ListGraphType<StringGraphType>, IEnumerable<string>>("urls")
                .Description(S["the absolute urls of the media items"])
                .PagingArguments()
                .Resolve(x =>
                {
                    if (x.Source?.Paths is null)
                    {
                        return Array.Empty<string>();
                    }
                    var paths = x.Page(x.Source.Paths);
                    var mediaFileStore = x.RequestServices.GetService<IMediaFileStore>();

                    return paths.Select(p => mediaFileStore.MapPathToPublicUrl(p));
                });

            Field<ListGraphType<StringGraphType>, IEnumerable<string>>("mediatexts")
            .Description(S["the media texts"])
            .PagingArguments()
            .Resolve(x =>
            {
                if (x.Source?.MediaTexts is null)
                {
                    return Array.Empty<string>();
                }
                return x.Page(x.Source.MediaTexts);
            });
        }

        Field<ListGraphType<MediaFileItemType>, IEnumerable<MediaFileItem>>("files")
            .Description(S["the files of the media items"])
            .PagingArguments()
            .Resolve(x =>
            {
                if (x.Source?.Paths is null)
                {
                    return [];
                }

                var paths = x.Page(x.Source.Paths).ToArray();
                var mediaFileStore = x.RequestServices.GetService<IMediaFileStore>();
                var urls = paths.Select(p => mediaFileStore.MapPathToPublicUrl(p)).ToArray();
                var fileNames = x.Page(x.Source?.GetAttachedFileNames()).ToArray();
                var items = new List<MediaFileItem>();
                var mediaTexts = x.Source?.MediaTexts ?? [];
                for (int i = 0; i < paths.Length; i++)
                {
                    items.Add(new MediaFileItem
                    {
                        Path = paths[i],
                        FileName = fileNames.Length > i ? fileNames[i] : string.Empty,
                        Url = urls.Length > i ? urls[i] : string.Empty,
                        MediaText = mediaTexts.Length > i ? mediaTexts[i] : string.Empty,
                    });
                }

                return items;
            });
    }
}

public sealed class MediaFileItemType : ObjectGraphType<MediaFileItem>
{
    public MediaFileItemType(IStringLocalizer<MediaFileItemType> S)
    {
        Field<StringGraphType>("fileName")
            .Description(S["the file name of the media file item"])
            .Resolve(x => x.Source.FileName);

        Field<StringGraphType>("path")
            .Description(S["the path of the media file item"])
            .Resolve(x => x.Source.Path);

        Field<StringGraphType>("url")
            .Description(S["the url name of the media file item"])
            .Resolve(x => x.Source.Url);

        Field<StringGraphType>("mediaText")
            .Description(S["the media text of the file item"])
            .Resolve(x => x.Source.MediaText);
    }
}

public sealed class MediaFileItem
{
    public string FileName { get; set; }
    public string Path { get; set; }
    public string Url { get; set; }
    public string MediaText { get; set; }
}
