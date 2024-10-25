using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries;

public class ContentItemQuery : ISchemaBuilder
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly IStringLocalizer S;

    public ContentItemQuery(IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<ContentItemQuery> localizer)
    {
        _httpContextAccessor = httpContextAccessor;

        S = localizer;
    }

    public Task<string> GetIdentifierAsync() => Task.FromResult(string.Empty);

    public Task BuildAsync(ISchema schema)
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
            Resolver = new FuncFieldResolver<ContentItem>(ResolveAsync)
        };

        schema.Query.AddField(field);

        return Task.CompletedTask;
    }

    private async ValueTask<ContentItem> ResolveAsync(IResolveFieldContext context)
    {
        var contentItemId = context.GetArgument<string>("contentItemId");
        var contentManager = _httpContextAccessor.HttpContext.RequestServices.GetService<IContentManager>();

        return await contentManager.GetAsync(contentItemId);
    }
}
