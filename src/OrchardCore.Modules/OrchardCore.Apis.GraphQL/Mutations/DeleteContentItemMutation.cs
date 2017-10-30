using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;

namespace OrchardCore.Apis.GraphQL.Mutations
{
    public class DeleteContentItemMutation : MutationFieldType
    {
        public DeleteContentItemMutation(IContentManager contentManager,
            IContentItemDisplayManager contentDisplay,
            IModelMetadataProvider metadataProvider,
            IModelBinderFactory modelBinderFactory,
            IHttpContextAccessor httpContextAccessor,
            IObjectModelValidator objectModelValidator)
        {
            Name = "DeleteContentItem";

            Arguments = new QueryArguments(
                new QueryArgument<StringGraphType> { Name = "contentItemId" }
            );

            Type = typeof(ContentItemType);

            Resolver = new SlowFuncFieldResolver<object, Task<DeletionStatus>>(async (context) => {
                var contentItemId = context.GetArgument<string>("contentItemId");

                var contentItem = await contentManager.GetAsync(contentItemId);

                await contentManager.RemoveAsync(contentItem);

                return new DeletionStatus { Status = "Ok" };
            });
        }
    }

    public class DeletionStatus {
        public string Status { get; set; }
    }
}
