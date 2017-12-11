using System;
using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Mutations;
using OrchardCore.Apis.GraphQL.Queries;

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
