using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Queries.GraphQL.Mutations
{
    public class DeleteQueryMutation : MutationFieldType
    {
        public DeleteQueryMutation(IHttpContextAccessor httpContextAccessor)
        {
            Name = "DeleteQuery";

            Arguments = new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }
                );

            Type = typeof(DeletionStatusObjectGraphType);

            Resolver = new AsyncFieldResolver<object, DeletionStatus>(async (context) =>
            {
                var name = context.GetArgument<string>("name");

                var queryManager = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IQueryManager>();
                await queryManager.DeleteQueryAsync(name);

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
