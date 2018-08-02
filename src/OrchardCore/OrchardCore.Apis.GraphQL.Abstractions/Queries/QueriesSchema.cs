using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public class QueriesSchema : ObjectGraphType
    {
        public QueriesSchema(IHttpContextAccessor httpContextAccessor)
        {
            Name = "Query";

            var fields = httpContextAccessor.HttpContext.RequestServices.GetServices<QueryFieldType>();

            foreach (var field in fields)
            {
                AddField(field);
            }

            var queryFieldTypeProviders = httpContextAccessor.HttpContext.RequestServices.GetServices<IQueryFieldTypeProvider>();

            foreach (var provider in queryFieldTypeProviders)
            {
                foreach (var queryField in provider.GetFields(this).GetAwaiter().GetResult())
                {
                    AddField(queryField);
                }
            }
        }
    }
}
