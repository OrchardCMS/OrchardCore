using System;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.RestApis.Queries
{
    public class ContentSchema : Schema
    {
        public ContentSchema(IServiceProvider serviceProvider)
            : base((type) => (IGraphType)serviceProvider.GetService(type))
        {
            Query = serviceProvider.GetService<ContentType>();
        }
    }
}
