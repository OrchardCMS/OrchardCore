using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Subscriptions
{
    public class SubscriptionSchema : ObjectGraphType
    {
        public SubscriptionSchema(IHttpContextAccessor httpContextAccessor)
        {
            Name = "Subscription";

            var fields = httpContextAccessor.HttpContext.RequestServices.GetServices<SubscriptionFieldType>();

            foreach (var field in fields)
            {
                AddField(field);
            }
        }
    }
}
