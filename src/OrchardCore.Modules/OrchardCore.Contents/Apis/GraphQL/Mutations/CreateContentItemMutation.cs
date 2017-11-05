using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL.Mutations;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.Contents.Apis.GraphQL.Mutations.Types;
using OrchardCore.Contents.Apis.GraphQL.Queries.Types;

namespace OrchardCore.Contents.Apis.GraphQL.Mutations
{
    public class CreateContentItemMutation : MutationFieldType
    {
        public CreateContentItemMutation(IContentManager contentManager,
            IContentItemDisplayManager contentDisplay,
            IApiUpdateModel apiUpdateModel)
        {
            Name = "CreateContentItem";

            Arguments = new QueryArguments(
                new QueryArgument<ContentItemInputType> { Name = "ContentItem" }
            );

            Type = typeof(ContentItemType);

            Resolver = new SlowFuncFieldResolver<object, Task<object>>(async (context) => {
                var contentItemFabrication = context.GetArgument<ContentItem>("ContentItem");

                var contentParts = JObject.Parse(
                    (context.Arguments["ContentItem"] as IDictionary<string, object>)["contentParts"].ToString());

                var contentItem = contentManager.New(contentItemFabrication.ContentType);

                contentItem.Author = contentItemFabrication.Author;
                contentItem.Owner = contentItemFabrication.Owner;

                var updateModel = apiUpdateModel.WithModel(contentParts);

                await contentDisplay.UpdateEditorAsync(contentItem, updateModel);

                if (contentItemFabrication.Published)
                {
                    await contentManager.PublishAsync(contentItem);
                }
                else
                {
                    contentManager.Create(contentItem);
                }

                return contentItem;

            });
        }
    }
}
