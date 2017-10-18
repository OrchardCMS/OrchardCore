using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.RestApis.Queries
{
    public class ContentSchema : Schema
    {
        public ContentSchema(IServiceProvider serviceProvider,
            IEnumerable<IObjectGraphType> objectGraphTypes)
            : base(new FuncDependencyResolver((type) => (IGraphType)serviceProvider.GetService(type)))
        {
            Mutation = serviceProvider.GetService<ContentItemMutation>();
            Query = serviceProvider.GetService<GraphQlQueryType>();

            RegisterTypes(objectGraphTypes.ToArray());
        }
    }
}
