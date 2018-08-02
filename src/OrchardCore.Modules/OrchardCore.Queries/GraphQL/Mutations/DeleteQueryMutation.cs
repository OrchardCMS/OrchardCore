using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Queries.GraphQL.Mutations
{
    public class DeleteQueryMutation : MutationFieldType
    {
        public IStringLocalizer T { get; set; }

        public DeleteQueryMutation(
            IHttpContextAccessor httpContextAccessor,
            IStringLocalizer<DeleteQueryMutation> t)
        {
            T = t;

            Name = "DeleteQuery";

            Arguments = new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "Name" }
                );

            Type = typeof(DeletionStatusObjectGraphType);

            Resolver = new AsyncFieldResolver<object, DeletionStatus>(async (context) =>
            {
                var queryManager = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IQueryManager>();

                var name = context.GetArgument<string>("Name");

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
