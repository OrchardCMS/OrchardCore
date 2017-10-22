using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.Apis.GraphQL.Mutations.Types;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Mutations
{
    public class CreateContentItemMutation : MutationFieldType
    {
        public CreateContentItemMutation(IContentManager contentManager,
            IContentItemDisplayManager contentDisplay,
            IModelMetadataProvider metadataProvider,
            IModelBinderFactory modelBinderFactory,
            IHttpContextAccessor httpContextAccessor,
            IObjectModelValidator objectModelValidator)
        {
            Name = "CreateContentItem";

            Arguments = new QueryArguments(
                new QueryArgument<ContentItemInputType> { Name = "contentItem" }
            );

            Type = typeof(ContentItemType);

            Resolver = new FuncFieldResolver<object, Task<object>>(async (context) => {
                var contentItemFabrication = context.GetArgument<ContentItem>("contentItem");

                var contentParts = JObject.Parse(
                    (context.Arguments["contentItem"] as IDictionary<string, object>)["contentParts"].ToString());

                var contentItem = contentManager.New(contentItemFabrication.ContentType);

                contentItem.Author = contentItemFabrication.Author;
                contentItem.Owner = contentItemFabrication.Owner;

                var updateModel = new ApiUpdateModel(metadataProvider, modelBinderFactory, httpContextAccessor, objectModelValidator, contentParts);

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
