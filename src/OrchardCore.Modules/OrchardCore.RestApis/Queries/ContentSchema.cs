using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.MetaData;
using OrchardCore.RestApis.Types;

namespace OrchardCore.RestApis.Queries
{
    public class ContentSchema : Schema
    {
        public ContentSchema(IServiceProvider serviceProvider,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<ContentPart> contentParts,
            IEnumerable<IObjectGraphType> objectGraphTypes)
            : base((type) => (IGraphType)serviceProvider.GetService(type))
        {
            var contentType = serviceProvider.GetService<ContentType>();

            var typeDefinitions = contentDefinitionManager.ListTypeDefinitions();

            foreach (var typeDefinition in typeDefinitions)
            {
                var typeType = new ObjectGraphType
                {
                    Name = typeDefinition.Name
                };

                foreach (var part in typeDefinition.Parts)
                {
                    var name = part.Name; // About
                    var partName = part.PartDefinition.Name; // BagPart

                    var contentPart = contentParts.FirstOrDefault(x => x.GetType().Name == partName);

                    if (contentPart != null)
                    {
                        var p = objectGraphTypes.FirstOrDefault(x => x.IsTypeOf(contentPart));

                        if (p != null)
                        {
                            typeType.AddField(new FieldType
                            {
                                //Type = contentPart.GetType(),
                                Name = name,
                                ResolvedType = (IObjectGraphType)serviceProvider.GetService(p.GetType())
                            });
                        }
                    }
                }

                contentType.AddField(new FieldType
                {
                    Name = typeDefinition.Name,
                    ResolvedType = typeType
                });
            }
            //AddField(new EventStreamFieldType
            //{
            //    Name = "messageAdded",
            //    Type = typeof(MessageType),
            //    Resolver = new EventStreamResolver(Subscribe)
            //});



            Query = contentType;
            RegisterType<TitlePartType>();
            RegisterType<AutoRoutePartType>();
            RegisterType<BagPartType>();
        }
    }

    public class AutoRegisteringObjectGraphType : ObjectGraphType
    {
        public AutoRegisteringObjectGraphType(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.PropertyType.GetTypeInfo().IsValueType || p.PropertyType == typeof(string));

            foreach (var propertyInfo in properties)
            {
                Field(propertyInfo.PropertyType.GetGraphTypeFromType(propertyInfo.PropertyType.IsNullable()), propertyInfo.Name);
            }
        }
    }
}
