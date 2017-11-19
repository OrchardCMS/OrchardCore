using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL.Arguments;
using OrchardCore.Apis.GraphQL.Mutations;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.Contents.Apis.GraphQL.Queries.Types;
using OrchardCore.Modules;

namespace OrchardCore.Contents.Apis.GraphQL.Mutations
{
    public class CreateContentItemMutation : MutationFieldType
    {
        public CreateContentItemMutation(IContentManager contentManager,
            IContentItemDisplayManager contentDisplay,
            IClock clock,
            IApiUpdateModel apiUpdateModel)
        {
            Name = "CreateContentItem";

            Arguments = new AutoRegisteringQueryArguments<ContentItem>
            {
                new QueryArgument<StringGraphType> { Name = "ContentParts" }
            };

            Type = typeof(ContentItemType);

            Resolver = new SlowFuncFieldResolver<object, Task<object>>(async (context) => {
                var contentItemFabrication = context.MapArgumentsTo<ContentItem>();
                var contentParts = JObject.Parse(context.GetArgument<string>("ContentParts"));

                var contentItem = contentManager.New(contentItemFabrication.ContentType);

                contentItem.Author = contentItemFabrication.Author;
                contentItem.Owner = contentItemFabrication.Owner;
                contentItem.CreatedUtc = clock.UtcNow;

                var updateModel = apiUpdateModel.WithModel(contentParts);

                await contentDisplay.UpdateEditorAsync(contentItem, updateModel, true);

                if (contentItemFabrication.Published)
                {
                    await contentManager.PublishAsync(contentItem);
                }
                else
                {
                    contentManager.Create(contentItem, VersionOptions.Latest);
                }

                return contentItem;
            });
        }
    }
}
