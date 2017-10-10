using System.Collections.Generic;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.RestApis.Queries.Types;
using OrchardCore.RestApis.Types;

namespace OrchardCore.RestApis.Queries
{
    public class ContentItemMutation : ObjectGraphType<object>
    {
        public ContentItemMutation(IContentManager contentManager,
            IContentItemDisplayManager contentDisplay,
            IModelMetadataProvider modelMetadataProvider)
        {
            Name = "Mutation";

            FieldAsync<ContentItemType>(
                "createContentItem",
                arguments: new QueryArguments(
                    new QueryArgument<ContentItemInputType> { Name = "contentItem" }
                ),
                resolve: async context =>
                {
                    var contentItemFabrication = context.GetArgument<ContentItem>("contentItem");

                    var contentParts = JObject.Parse(
                        (context.Arguments["contentItem"] as IDictionary<string, object>)["contentParts"].ToString());

                    var contentItem = contentManager.New(contentItemFabrication.ContentType);

                    contentItem.Author = contentItemFabrication.Author;
                    contentItem.Owner = contentItemFabrication.Owner;

                    var updateModel = new ApiUpdateModel(modelMetadataProvider, contentParts);

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
