using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Mutations;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.Subscriptions;

namespace OrchardCore.Apis.GraphQL
{
    public class ContentSchema : Schema
    {
        public ContentSchema(
            MutationsSchema mutationsSchema,
            QueriesSchema queriesSchema,
            SubscriptionSchema subscriptionSchema,
            IEnumerable<IInputObjectGraphType> inputGraphTypes,
            IEnumerable<IObjectGraphType> queryGraphTypes,
            IDependencyResolver dependencyResolver)
            : base(dependencyResolver)
        {
            Mutation = mutationsSchema;
            Query = queriesSchema;

            var subscription = subscriptionSchema;

            if (subscription.Fields.Any())
            {
                Subscription = subscription;
            }

            foreach (var type in inputGraphTypes)
            {
                RegisterType(type);
            }

            foreach (var type in queryGraphTypes)
            {
                RegisterType(type);
            }
        }
    }
}
