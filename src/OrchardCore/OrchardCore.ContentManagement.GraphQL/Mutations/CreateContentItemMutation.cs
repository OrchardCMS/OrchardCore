using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateContentItemMutation(
            IHttpContextAccessor httpContextAccessor,
            IClock clock)
        {
            Name = "CreateContentItem";

            Arguments = new QueryArguments
            {
                new QueryArgument<NonNullGraphType<CreateContentItemInputType>> { Name = "ContentItem" }
            };

            Type = typeof(ContentItemType);

            Resolver = new AsyncFieldResolver<object, object>(async (context) =>
            {
                var contentItemFabrication = context.GetArgument<ContentItemInput>("ContentItem");

                var contentParts = JObject.FromObject(contentItemFabrication.ContentParts);

                var contentManager = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IContentManager>();
                var contentItem = await contentManager.NewAsync(contentItemFabrication.ContentType);

                contentItem.Author = contentItemFabrication.Author;
                contentItem.Owner = contentItemFabrication.Owner;
                contentItem.CreatedUtc = clock.UtcNow;

                var apiUpdateModel = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IApiUpdateModel>();
                var updateModel = apiUpdateModel.WithModel(contentParts);

                var contentDisplay = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IContentItemDisplayManager>();
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
            _httpContextAccessor = httpContextAccessor;
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
