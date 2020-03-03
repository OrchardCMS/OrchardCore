using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Resolvers;
using OrchardCore.FileStorage;

namespace OrchardCore.Media.GraphQL
{
    public class MediaAssetQuery : ISchemaBuilder
    {
        private readonly IStringLocalizer S;

        public MediaAssetQuery(IStringLocalizer<MediaAssetQuery> localizer)
        {
            S = localizer;
        }

        public Task<IChangeToken> BuildAsync(ISchema schema)
        {
            var field = new FieldType
            {
                Name = "MediaAssets",
                Description = S["Media assets are items that are part of your media library."],
                Type = typeof(ListGraphType<MediaAssetObjectType>),
                Arguments = new QueryArguments(
                    new QueryArgument<StringGraphType>
                    {
                        Name = "path",
                        Description = S["Media asset path."]
                    },
                    new QueryArgument<BooleanGraphType>
                    {
                        Name = "includeSubDirectories",
                        Description = S["Whether to get the assets from just the top directory or from all sub-directories as well."]
                    }
                ),
                Resolver = new LockedAsyncFieldResolver<IEnumerable<IFileStoreEntry>>(ResolveAsync)
            };

            schema.Query.AddField(field);

            return Task.FromResult<IChangeToken>(null);
        }

        private async Task<IEnumerable<IFileStoreEntry>> ResolveAsync(ResolveFieldContext resolveContext)
        {
            var mediaFileStore = resolveContext.ResolveServiceProvider().GetService<IMediaFileStore>();

            var path = resolveContext.GetArgument("path", string.Empty);
            var includeSubDirectories = resolveContext.GetArgument("includeSubDirectories", false);

            var allFiles = await mediaFileStore.GetDirectoryContentAsync(path, includeSubDirectories);

            if (includeSubDirectories)
            {
                return allFiles;
            }
            else
            {
                return allFiles.Where(x => !x.IsDirectory);
            }
        }
    }
}
