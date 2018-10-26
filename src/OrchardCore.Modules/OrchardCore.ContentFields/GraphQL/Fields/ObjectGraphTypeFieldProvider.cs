using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.ContentFields.GraphQL
{
    public class ObjectGraphTypeFieldProvider : IContentFieldProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ObjectGraphTypeFieldProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public FieldType GetField(ContentPartFieldDefinition field)
        {
            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;
            var typeActivator = serviceProvider.GetService<ITypeActivatorFactory<ContentField>>();
            var activator = typeActivator.GetTypeActivator(field.FieldDefinition.Name);

            var queryGraphType = typeof(ObjectGraphType<>).MakeGenericType(activator.Type);

            if (serviceProvider.GetService(queryGraphType) is IObjectGraphType queryGraphTypeResolved)
            {
                return new FieldType
                {
                    Name = field.Name,
                    Description = field.FieldDefinition.Name,
                    Type = queryGraphType,
                    Resolver = new FuncFieldResolver<ContentItem, ContentElement>(context =>
                    {
                        var typeToResolve = context.ReturnType.GetType().BaseType.GetGenericArguments().First();

                        var contentPart = context.Source.Get(typeof(ContentPart), field.PartDefinition.Name);
                        var contentField = contentPart.Get(typeToResolve, context.FieldName.FirstCharToUpper());
                        return contentField;
                    })
                };
            }

            return null;
        }
    }
}