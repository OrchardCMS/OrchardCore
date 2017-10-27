using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public class ContentItemsQuery : QueryFieldType
    {
        public ContentItemsQuery(IContentManager contentManager,
            IEnumerable<ContentPart> contentParts,
            ISession session)
        {
            Name = "ContentItems";

            Type = typeof(ListGraphType<ContentItemType>);

            Arguments = new QueryArguments(
                new QueryArgument<IntGraphType> { Name = "id", Description = "id of the content item" },
                new QueryArgument<BooleanGraphType> { Name = "published", Description = "is the content item published", DefaultValue = true },
                new QueryArgument<StringGraphType> { Name = "latest", Description = "is the content item the latest version", DefaultValue = true },
                new QueryArgument<IntGraphType> { Name = "number", Description = "version number, 1,2,3 etc" },
                new QueryArgument<StringGraphType> { Name = "contentType", Description = "type of content item" },
                new QueryArgument<StringGraphType> { Name = "contentItemId", Description = "same as id" },
                new QueryArgument<StringGraphType> { Name = "contentItemVersionId", Description = "the id of the version" }
            );

            Resolver = new FuncFieldResolver<Task<IEnumerable<ContentItem>>>(async context => {
                if (context.HasPopulatedArgument("contentItemId"))
                {
                    return new[] { await contentManager.GetAsync(context.GetArgument<string>("contentItemId")) };
                }

                var isPublished = context.GetArgument<bool>("published");
                var isLatest = context.GetArgument<bool>("latest");

                var query = session.Query<ContentItem, ContentItemIndex>().Where(q =>
                    q.Published == isPublished &&
                    q.Latest == isLatest);

                if (context.HasPopulatedArgument("id"))
                {
                    var value = context.GetArgument<int>("id");
                    query = query.Where(q => q.Id == value);
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
                else
                {
                    var value = (context.ReturnType as ListGraphType).ResolvedType.Name;
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

                var contentItems = await query.ListAsync();

                foreach (var argument in context.Arguments)
                {
                    if (argument.Value != null)
                    {
                        var nestedValues = argument.Value as IDictionary<string, object>;

                        if (nestedValues != null)
                        {
                            var contentPart = contentParts.First(cp => cp.GetType().Name.ToGraphQLStringFormat() == argument.Key);
                            var contentPartType = contentPart.GetType();

                            foreach (var nestedValue in nestedValues)
                            {
                                contentItems = contentItems.Where(ci => {
                                    var foundPart = ci
                                        .Get(contentPartType, contentPartType.Name)
                                        .AsDictionary()
                                        .First(k => k.Key.ToGraphQLStringFormat() == nestedValue.Key);

                                    return foundPart.Value.Equals(nestedValue.Value);
                                });
                            }
                        }
                    }
                }

                var items = contentItems.ToList();

                return items;
            });
        }
    }
}
