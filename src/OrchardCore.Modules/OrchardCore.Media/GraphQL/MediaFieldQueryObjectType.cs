using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Media.Core.Processing;
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

        Field<StringGraphType>("resizeUrl")
            .Description(S["the url of the media file item with image processing parameters"])
            .Argument<StringGraphType>("profile", arg => arg.Description = S["the media profile name to use for image processing"])
            .Argument<IntGraphType>("width", arg => arg.Description = S["the width of the image"])
            .Argument<IntGraphType>("height", arg => arg.Description = S["the height of the image"])
            .Argument<StringGraphType>("mode", arg => arg.Description = S["the resize mode (pad, boxpad, max, min, stretch)"])
            .Argument<IntGraphType>("quality", arg => arg.Description = S["the quality of the image (1-100)"])
            .Argument<StringGraphType>("format", arg => arg.Description = S["the format of the image (png, jpg, gif, bmp, webp)"])
            .Argument<StringGraphType>("bgcolor", arg => arg.Description = S["the background color of the image"])
            .Argument<BooleanGraphType>("autoorient", arg => arg.Description = S["auto-orient the image based on EXIF data (defaults to true)"])
            .ResolveAsync(async x =>
            {
                if (string.IsNullOrEmpty(x.Source.Path))
                {
                    return x.Source.Url;
                }

                var profile = x.GetArgument<string>("profile");
                var width = x.GetArgument<int?>("width");
                var height = x.GetArgument<int?>("height");
                var mode = x.GetArgument<string>("mode");
                var quality = x.GetArgument<int?>("quality");
                var format = x.GetArgument<string>("format");
                var bgcolor = x.GetArgument<string>("bgcolor");
                var autoorient = x.GetArgument<bool?>("autoorient");

                var orchardHelper = x.RequestServices.GetRequiredService<IOrchardHelper>();

                var resizeMode = !string.IsNullOrEmpty(mode) && Enum.TryParse<ResizeMode>(mode, ignoreCase: true, out var parsed)
                    ? parsed
                    : ResizeMode.Undefined;

                var imageFormat = !string.IsNullOrEmpty(format) && Enum.TryParse<Format>(format, ignoreCase: true, out var parsedFormat)
                    ? parsedFormat
                    : Format.Undefined;

                string assetUrl;
                if (!string.IsNullOrEmpty(profile))
                {
                    assetUrl = await orchardHelper.AssetProfileUrlAsync(
                        x.Source.Path,
                        profile,
                        width,
                        height,
                        resizeMode,
                        appendVersion: true,
                        quality,
                        imageFormat,
                        bgcolor: bgcolor,
                        autoorient: autoorient);
                }
                else
                {
                    assetUrl = orchardHelper.AssetUrl(
                        x.Source.Path,
                        width,
                        height,
                        resizeMode,
                        appendVersion: true,
                        quality,
                        imageFormat,
                        bgColor: bgcolor,
                        autoorient: autoorient);
                }

                // Encode spaces as %20 to ensure the URL is valid, as GraphQL does not automatically encode them.
                return assetUrl?.Replace(" ", "%20");
            });

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
