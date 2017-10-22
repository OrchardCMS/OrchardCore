using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public class ContentItemsQuery : QueryFieldType
    {
        public ContentItemsQuery(IContentManager contentManager,
            ISession session) {
            Name = "ContentItems";

            Type = typeof(ListGraphType<ContentItemType>);

            Arguments = new QueryArguments(
                    new QueryArgument<IntGraphType> { Name = "id", Description = "id of the content item" },
                    new QueryArgument<BooleanGraphType> { Name = "published", Description = "is the content item published" },
                    new QueryArgument<StringGraphType> { Name = "latest", Description = "is the content item the latest version" },
                    new QueryArgument<IntGraphType> { Name = "number", Description = "version number, 1,2,3 etc" },
                    new QueryArgument<StringGraphType> { Name = "contentType", Description = "type of content item" },
                    new QueryArgument<StringGraphType> { Name = "contentItemId", Description = "content item id" },
                    new QueryArgument<StringGraphType> { Name = "contentItemVersionId", Description = "the id of the version" }
                );

            Resolver = new FuncFieldResolver<object, Task<object>>(async (context) => {
                if (context.HasPopulatedArgument("contentItemId"))
                {
                    return new[] { await contentManager.GetAsync(context.GetArgument<string>("contentItemId")) };
                }

                var query = session.Query<ContentItem, ContentItemIndex>();

                //foreach (var argument in context.Arguments.Where(qa => qa.Value != null))
                //{
                //    query = query.WithParameter(argument.Key, argument.Value);
                //}

                if (context.HasPopulatedArgument("id"))
                {
                    var value = context.GetArgument<int>("id");
                    query = query.Where(q => q.Id == value);
                }

                if (context.HasPopulatedArgument("published"))
                {
                    var value = context.GetArgument<bool>("published");
                    query = query.Where(q => q.Published == value);
                }

                if (context.HasPopulatedArgument("latest"))
                {
                    var value = context.GetArgument<bool>("latest");
                    query = query.Where(q => q.Latest == value);
                }

                if (context.HasPopulatedArgument("number"))
                {
                    var value = context.GetArgument<int>("number");
                    query = query.Where(q => q.Number == value);
                }

                if (context.HasPopulatedArgument("contentType"))
                {
                    var value = context.GetArgument<string>("contentType");
                    query = query.Where(q => q.ContentType == value);
                }

                if (context.HasPopulatedArgument("contentItemId"))
                {
                    var value = context.GetArgument<string>("contentItemId");
                    query = query.Where(q => q.ContentItemId == value);
                }

                if (context.HasPopulatedArgument("contentItemVersionId"))
                {
                    var value = context.GetArgument<string>("contentItemVersionId");
                    query = query.Where(q => q.ContentItemVersionId == value);
                }

                return await query.ListAsync();
            });
        }
    }
}
