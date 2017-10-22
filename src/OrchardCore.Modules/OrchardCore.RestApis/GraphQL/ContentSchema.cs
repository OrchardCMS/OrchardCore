using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.RestApis.GraphQL.Mutations;
using OrchardCore.RestApis.GraphQL.Queries;

namespace OrchardCore.RestApis.Queries
{
    public class ContentSchema : Schema
    {
        public ContentSchema(IServiceProvider serviceProvider,
            IEnumerable<IObjectGraphType> objectGraphTypes)
            : base(new FuncDependencyResolver((type) => (IGraphType)serviceProvider.GetService(type)))
        {
            Mutation = serviceProvider.GetService<MutationsSchema>();
            Query = serviceProvider.GetService<QueriesSchema>();

            RegisterTypes(objectGraphTypes.ToArray());
        }
    }
}
