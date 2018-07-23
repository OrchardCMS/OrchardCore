using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL.Mutations;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.GraphQL.Mutations.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Modules;

namespace OrchardCore.ContentManagement.GraphQL.Mutations
{
    public class CreateContentItemMutation : MutationFieldType
    {
        public CreateContentItemMutation(IContentManager contentManager,
            IContentItemDisplayManager contentDisplay,
            IAuthorizationService authorizationService,
            IClock clock,
            IApiUpdateModel apiUpdateModel)
        {
            Name = "CreateContentItem";

            Arguments = new QueryArguments
            {
                new QueryArgument<NonNullGraphType<CreateContentItemInputType>> { Name = "ContentItem" }
            };

            Type = typeof(ContentItemType);

            Resolver = new AsyncFieldResolver<object, object>(async (context) => {
                var contentItemFabrication = context.GetArgument<ContentItemInput>("ContentItem");
                
                var contentParts = JObject.FromObject(contentItemFabrication.ContentParts);

                var contentItem = await contentManager.NewAsync(contentItemFabrication.ContentType);

                contentItem.Author = contentItemFabrication.Author;
                contentItem.Owner = contentItemFabrication.Owner;
                contentItem.CreatedUtc = clock.UtcNow;

                var updateModel = apiUpdateModel.WithModel(contentParts);

                await contentDisplay.UpdateEditorAsync(contentItem, updateModel, true);

                if (contentItemFabrication.Published)
                {
                    // TODO : Auth check for publish
                    await contentManager.CreateAsync(contentItem, VersionOptions.Published);
                }
                else
                {
                    await contentManager.CreateAsync(contentItem, VersionOptions.Latest);
                }

                return contentItem;
            });
        }

        private class ContentItemInput
        {
            public string ContentType { get; set; }
            public string Author { get; set; }
            public string Owner { get; set; }
            public bool Published { get; set; }
            public bool Latest { get; set; }

            public IDictionary<string, object> ContentParts { get; set; } = new Dictionary<string, object>();
        }

        public class ContentPartInput
        {
            public string Name { get; set; }
            public string Content { get; set; }
        }
    }
}
