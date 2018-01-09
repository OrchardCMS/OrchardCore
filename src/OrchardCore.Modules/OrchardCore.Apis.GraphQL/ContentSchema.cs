using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Mutations;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.Subscriptions;

namespace OrchardCore.Apis.GraphQL
{
    public class ContentSchema : Schema
    {
        public ContentSchema(IServiceProvider serviceProvider,
            IEnumerable<IInputObjectGraphType> inputGraphTypes,
            IEnumerable<IObjectGraphType> queryGraphTypes,
            IDependencyResolver dependencyResolver)
            : base(dependencyResolver)
        {
            Mutation = serviceProvider.GetService<MutationsSchema>();
            Query = serviceProvider.GetService<QueriesSchema>();

            var subscription = serviceProvider.GetService<SubscriptionSchema>();

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
