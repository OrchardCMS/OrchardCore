using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Resolvers;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.FileStorage;

namespace OrchardCore.Media.GraphQL
{
    public class MediaAssetQuery : ISchemaBuilder
    {
        protected readonly IStringLocalizer S;
        private readonly GraphQLContentOptions _graphQLContentOptions;

        public MediaAssetQuery(
            IStringLocalizer<MediaAssetQuery> localizer,
            IOptions<GraphQLContentOptions> graphQLContentOptions)
        {
            S = localizer;
            _graphQLContentOptions = graphQLContentOptions.Value;
        }

        public Task<string> GetIdentifierAsync() => Task.FromResult(String.Empty);

        public Task BuildAsync(ISchema schema)
        {
            if (_graphQLContentOptions.IsHiddenByDefault("MediaAssets"))
            {
                return Task.CompletedTask;
            }

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

            return Task.CompletedTask;
        }

        private async Task<IEnumerable<IFileStoreEntry>> ResolveAsync(IResolveFieldContext resolveContext)
        {
            var mediaFileStore = resolveContext.RequestServices.GetService<IMediaFileStore>();

            var path = resolveContext.GetArgument("path", String.Empty);
            var includeSubDirectories = resolveContext.GetArgument("includeSubDirectories", false);

            var allFiles = mediaFileStore.GetDirectoryContentAsync(path, includeSubDirectories);

            if (includeSubDirectories)
            {
                return await allFiles.ToListAsync();
            }
            else
            {
                return await allFiles.Where(x => !x.IsDirectory).ToListAsync();
            }
        }
    }
}
