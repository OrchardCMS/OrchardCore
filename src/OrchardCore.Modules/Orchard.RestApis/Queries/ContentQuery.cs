using GraphQL.Types;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.RestApis.Types;

namespace Orchard.RestApis.Queries
{
    public class ContentQuery : ObjectGraphType<object>
    {
        public ContentQuery(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager)
        {
            Name = "contentquery";

            Field<ContentItemType>(
              "contentitem",
              arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id of the content items" }
                ),
              resolve: context => contentManager.GetAsync(context.GetArgument<string>("id")).GetAwaiter().GetResult()
            );

            Field<ListGraphType<ContentTypeType>>(
                "contenttypes",
                resolve: context => contentDefinitionManager.ListTypeDefinitions()
                );
        }
    }
}
