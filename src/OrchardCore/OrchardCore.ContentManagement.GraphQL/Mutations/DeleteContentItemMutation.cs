using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentManagement.GraphQL.Mutations
{
    public class DeleteContentItemMutation : MutationFieldType
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeleteContentItemMutation(IHttpContextAccessor httpContextAccessor)
        {
            Name = "DeleteContentItem";

            Arguments = new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ContentItemId" }
            );

            Type = typeof(DeletionStatusObjectGraphType);

            Resolver = new AsyncFieldResolver<object, DeletionStatus>(async (context) =>
            {
                var contentItemId = context.GetArgument<string>("ContentItemId");

                var contentManager = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IContentManager>();
                var contentItem = await contentManager.GetAsync(contentItemId);

                //if (!await authorizationService.AuthorizeAsync((context.UserContext as GraphQLUserContext)?.User, Permissions.DeleteContent, contentItem))
                //{
                //    return null;
                //}

                if (contentItem != null)
                {
                    await contentManager.RemoveAsync(contentItem);
                }

                return new DeletionStatus { Status = "Ok" };
            });
            _httpContextAccessor = httpContextAccessor;
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
