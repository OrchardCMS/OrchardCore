using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public class ContentItemQuery : ISchemaBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStringLocalizer S;

        public ContentItemQuery(IHttpContextAccessor httpContextAccessor,
            IStringLocalizer<ContentItemQuery> localizer)
        {
            _httpContextAccessor = httpContextAccessor;

            S = localizer;
        }

        public Task<IChangeToken> BuildAsync(ISchema schema)
        {
            var field = new FieldType
            {
                Name = "ContentItem",
                Description = S["Content items are instances of content types, just like objects are instances of classes."],
                Type = typeof(ContentItemInterface),
                Arguments = new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>>
                    {
                        Name = "contentItemId",
                        Description = S["Content item id"]
                    }
                ),
                Resolver = new AsyncFieldResolver<ContentItem>(ResolveAsync)
            };

            schema.Query.AddField(field);

            return Task.FromResult<IChangeToken>(null);
        }

        private Task<ContentItem> ResolveAsync(ResolveFieldContext context)
        {
            var contentItemId = context.GetArgument<string>("contentItemId");
            var contentManager = _httpContextAccessor.HttpContext.RequestServices.GetService<IContentManager>();
            return contentManager.GetAsync(contentItemId);
        }
    }
}
