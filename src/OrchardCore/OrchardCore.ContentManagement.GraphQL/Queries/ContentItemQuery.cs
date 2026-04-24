using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries;

public sealed class ContentItemQuery : ISchemaBuilder
{
    internal readonly IStringLocalizer S;

    public ContentItemQuery(IStringLocalizer<ContentItemQuery> localizer)
    {
        S = localizer;
    }

    public Task<string> GetIdentifierAsync()
        => Task.FromResult(string.Empty);

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
                    Description = S["Content item id"],
                }
            ),
            Resolver = new FuncFieldResolver<ContentItem>(ResolveAsync),
        };

        field.RequirePermission(GraphQLPermissions.ExecuteGraphQL);

        schema.Query.AddField(field);

        return Task.CompletedTask;
    }

    private async ValueTask<ContentItem> ResolveAsync(IResolveFieldContext context)
    {
        var contentItemId = context.GetArgument<string>("contentItemId");
        var contentManager = context.RequestServices.GetService<IContentManager>();
        var authorizationService = context.RequestServices.GetService<IAuthorizationService>();

        var contentItem = await contentManager.GetAsync(contentItemId);

        if (contentItem == null)
        {
            return null;
        }

        if (!await authorizationService.AuthorizeAsync(context.User, Contents.CommonPermissions.ViewContent, contentItem))
        {
            // Return null if the user doesn't have permission to view the content item, so that it doesn't appear in the GraphQL response.
            return null;
        }

        return contentItem;
    }
}
