using System.Linq;
using GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.MetaData;
using OrchardCore.ContentManagement.Records;
using OrchardCore.RestApis.Types;
using YesSql;

namespace OrchardCore.RestApis.Queries
{
    public class ContentType : ObjectGraphType
    {
        public ContentType(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            ISession session)
        {
            Name = "content";

            FieldAsync<ContentItemType>(
              "contentitem",
              arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id of the content item" }
                ),
              resolve: async context => await contentManager.GetAsync(context.GetArgument<string>("id"))
            );

            FieldAsync<ListGraphType<ContentItemType>>(
              "contentitems",
              arguments: new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "id", Description = "id of the content item" },
                    new QueryArgument<BooleanGraphType> { Name = "published", Description = "is the content item published" },
                    new QueryArgument<StringGraphType> { Name = "latest", Description = "is the content item the latest version" },
                    new QueryArgument<IntGraphType> { Name = "number", Description = "version number, 1,2,3 etc" },
                    new QueryArgument<StringGraphType> { Name = "contentType", Description = "type of content item" },
                    new QueryArgument<StringGraphType> { Name = "contentItemId", Description = "same as id" },
                    new QueryArgument<StringGraphType> { Name = "contentItemIVersionId", Description = "the id of the version" }
                ),
              resolve: async context => {
                  if (context.HasPopulatedArgument("id"))
                  {
                      return new[] { await contentManager.GetAsync(context.GetArgument<string>("id")) };
                  }

                  var query = session.Query<ContentItem, ContentItemIndex>();

                  //foreach (var argument in context.Arguments.Where(qa => qa.Value != null))
                  //{
                  //    query = query.WithParameter(argument.Key, argument.Value);
                  //}


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
              }
            );

            Field<ListGraphType<ContentTypeType>>(
                "contenttypes",
                resolve: context => contentDefinitionManager.ListTypeDefinitions()
                );
        }
    }

    public static class ResolveFieldContextExtensions
    {
        public static bool HasPopulatedArgument<TSource>(this ResolveFieldContext<TSource> source, string argumentName)
        {
            if (source.Arguments?.ContainsKey(argumentName) ?? false)
            {
                return !string.IsNullOrEmpty(source.Arguments[argumentName]?.ToString());
            };

            return false;
        }
    }
}
