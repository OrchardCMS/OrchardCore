using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Mutations
{
    public class MutationsSchema : ObjectGraphType
    {
        public MutationsSchema(IHttpContextAccessor httpContextAccessor)
        {
            Name = "Mutations";

            var fields = httpContextAccessor.HttpContext.RequestServices.GetServices<MutationFieldType>();

            foreach (var field in fields)
            {
                AddField(field);
            }
        }
    }
}
