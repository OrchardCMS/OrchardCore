using System.Collections.Generic;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Subscriptions
{
    public class SubscriptionSchema : ObjectGraphType
    {
        public SubscriptionSchema(IEnumerable<SubscriptionFieldType> fields)
        {
            Name = "Subscription";

            foreach (var field in fields)
            {
                AddField(field);
            }
        }
    }
}
