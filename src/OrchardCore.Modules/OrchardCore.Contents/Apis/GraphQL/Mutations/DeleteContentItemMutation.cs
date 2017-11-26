using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Apis.GraphQL.Mutations
{
    public class DeleteContentItemMutation : MutationFieldType
    {
        public DeleteContentItemMutation(IContentManager contentManager)
        {
            Name = "DeleteContentItem";

            Arguments = new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ContentItemId" }
            );

            Type = typeof(DeletionStatusObjectGraphType);

            Resolver = new SlowFuncFieldResolver<object, Task<DeletionStatus>>(async (context) => {
                var contentItemId = context.GetArgument<string>("ContentItemId");

                var contentItem = await contentManager.GetAsync(contentItemId);

                if (contentItem != null)
                {
                    await contentManager.RemoveAsync(contentItem);
                }

                return new DeletionStatus { Status = "Ok" };
            });
        }
    }

    public class DeletionStatus : GraphType
    {
        public string Status { get; set; }
    }

    public class DeletionStatusObjectGraphType : AutoRegisteringObjectGraphType<DeletionStatus>
    {
    }
}
