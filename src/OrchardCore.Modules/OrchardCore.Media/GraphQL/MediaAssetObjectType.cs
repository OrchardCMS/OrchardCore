using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.FileStorage;

namespace OrchardCore.Media.GraphQL;

public class MediaAssetObjectType : ObjectGraphType<IFileStoreEntry>
{
    public MediaAssetObjectType(IStringLocalizer<MediaAssetObjectType> S)
    {
        Name = "MediaAsset";

        Field(file => file.Name).Description(S["The name of the asset."]);

        Field<StringGraphType>("path")
            .Description(S["The url to the asset."])
            .Resolve(x =>
            {
                var path = x.Source.Path;
                var mediaFileStore = x.RequestServices.GetService<IMediaFileStore>();
                return mediaFileStore.MapPathToPublicUrl(path);
            });

        Field(file => file.Length)
            .Description(S["The length of the file."]);

        Field<DateTimeGraphType>("lastModifiedUtc")
            .Description(S["The date and time in UTC when the asset was last modified."])
            .Resolve(file => file.Source.LastModifiedUtc);
    }
}
