using System;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.RestApis.Types;

namespace OrchardCore.RestApis.Queries
{
    public class ContentSchema : Schema
    {
        public ContentSchema(IServiceProvider serviceProvider)
            : base((type) => (IGraphType)serviceProvider.GetService(type))
        {
            var contentType = serviceProvider.GetService<ContentType>();



            //AddField(new EventStreamFieldType
            //{
            //    Name = "messageAdded",
            //    Type = typeof(MessageType),
            //    Resolver = new EventStreamResolver(Subscribe)
            //});



            Query = serviceProvider.GetService<ContentType>();
            RegisterType<TitlePartType>();
            RegisterType<AutoRoutePartType>();
            RegisterType<BagPartType>();
        }
    }
}
