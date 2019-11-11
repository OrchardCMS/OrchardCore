using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;
using OrchardCore.Apis.GraphQL;
using OrchardCore.FileStorage;

namespace OrchardCore.Media.GraphQL
{
    public class MediaAssetQuery : ISchemaBuilder
    {
        public MediaAssetQuery(IStringLocalizer<MediaAssetQuery> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; }

        public Task<IChangeToken> BuildAsync(ISchema schema)
        {
            var field = new FieldType
            {
                Name = "MediaAssets",
                Description = T["Media assets are items that are part of your media library."],
                Type = typeof(ListGraphType<MediaAssetObjectType>),
                Arguments = new QueryArguments(
                    new QueryArgument<StringGraphType>
                    {
                        Name = "path",
                        Description = T["Media asset path."]
                    },
                    new QueryArgument<BooleanGraphType>
                    {
                        Name = "includeSubDirectories",
                        Description = T["Whether to get the assets from just the top directory or from all sub-directories as well."]
                    }
                ),
                Resolver = new AsyncFieldResolver<IEnumerable<IFileStoreEntry>>(ResolveAsync)
            };

            schema.Query.AddField(field);

            return Task.FromResult<IChangeToken>(null);
        }

        private async Task<IEnumerable<IFileStoreEntry>> ResolveAsync(ResolveFieldContext resolveContext)
        {
            var context = (GraphQLContext)resolveContext.UserContext;
            var mediaFileStore = context.ServiceProvider.GetService<IMediaFileStore>();

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