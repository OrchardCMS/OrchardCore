using System.Collections.Concurrent;
using System;
using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.GraphQL.Fields
{
    public class ObjectGraphTypeFieldProvider : IContentFieldProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ConcurrentDictionary<string, Type> ContentTypeTypes = new();
        private readonly ConcurrentDictionary<Type, Type> ObjectGraphTypePartTypes = new();
        public ObjectGraphTypeFieldProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public FieldType GetField(ContentPartFieldDefinition field)
        {
            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;
            var typeActivator = serviceProvider.GetService<ITypeActivatorFactory<ContentField>>();
            var contentTypeType = ContentTypeTypes.GetOrAdd(field.FieldDefinition.Name, key => typeActivator.GetTypeActivator(key).Type);

            var queryGraphType = ObjectGraphTypePartTypes.GetOrAdd(contentTypeType, key => typeof(ObjectGraphType<>).MakeGenericType(key));

            if (serviceProvider.GetService(queryGraphType) is IObjectGraphType)
            {
                return new FieldType
                {
                    Name = field.Name,
                    Description = field.FieldDefinition.Name,
                    Type = queryGraphType,
                    Resolver = new FuncFieldResolver<ContentElement, ContentElement>(context =>
                    {
                        var typeToResolve = context.FieldDefinition.ResolvedType.GetType().BaseType.GetGenericArguments().First();

                        // Check if part has been collapsed by trying to get the parent part.
                        var contentPart = context.Source.Get(typeof(ContentPart), field.PartDefinition.Name);

                        // Part is not collapsed, access field directly.
                        contentPart ??= context.Source;

                        var contentField = contentPart?.Get(typeToResolve, field.Name);
                        return contentField;
                    })
                };
            }

            return null;
        }
    }
}
