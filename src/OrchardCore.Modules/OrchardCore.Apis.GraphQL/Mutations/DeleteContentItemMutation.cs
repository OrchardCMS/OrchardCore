using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using OrchardCore.ContentManagement;

namespace OrchardCore.Apis.GraphQL.Mutations
{
    public class DeleteContentItemMutation : MutationFieldType
    {
        public DeleteContentItemMutation(IContentManager contentManager)
        {
            Name = "DeleteContentItem";

            Arguments = new QueryArguments(
                new QueryArgument<StringGraphType> { Name = "contentItemId" }
            );

            Type = typeof(DeletionStatus);

            Resolver = new SlowFuncFieldResolver<object, Task<DeletionStatus>>(async (context) => {
                var contentItemId = context.GetArgument<string>("contentItemId");

                var contentItem = await contentManager.GetAsync(contentItemId);

                await contentManager.RemoveAsync(contentItem);

                return new DeletionStatus { Status = "Ok" };
            });
        }
    }

    public class DeletionStatus : GraphType
    {
        public string Status { get; set; }
    }
}
