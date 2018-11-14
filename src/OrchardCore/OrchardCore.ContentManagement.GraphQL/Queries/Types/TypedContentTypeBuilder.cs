using System.Linq;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public class TypedContentTypeBuilder : IContentTypeBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TypedContentTypeBuilder(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Build(FieldType contentQuery, ContentTypeDefinition contentTypeDefinition, ContentItemType contentItemType)
        {
            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;
            var typeActivator = serviceProvider.GetService<ITypeActivatorFactory<ContentPart>>();

            foreach (var part in contentTypeDefinition.Parts)
            {
                var partName = part.Name;

                // Check if another builder has already added a field for this part.
                if (contentItemType.HasField(partName))
                {
                    continue;
                }

                var activator = typeActivator.GetTypeActivator(part.PartDefinition.Name);

                var queryGraphType = typeof(ObjectGraphType<>).MakeGenericType(activator.Type);

                if (serviceProvider.GetService(queryGraphType) is IObjectGraphType queryGraphTypeResolved)
                {
                    contentItemType.Field(
                        queryGraphTypeResolved.GetType(),
                        partName,
                        resolve: context =>
                        {
                            var nameToResolve = partName;
                            var typeToResolve = context.ReturnType.GetType().BaseType.GetGenericArguments().First();

                            return context.Source.Get(typeToResolve, nameToResolve);
                        });
                }

                var inputGraphType = typeof(InputObjectGraphType<>).MakeGenericType(activator.Type);

                if (serviceProvider.GetService(inputGraphType) is IInputObjectGraphType inputGraphTypeResolved)
                {
                    var whereArgument = contentQuery.Arguments.FirstOrDefault(x => x.Name == "where");
                    if (whereArgument == null)
                    {
                        return;
                    }

                    var whereInput = (ContentItemWhereInput) whereArgument.ResolvedType;

                    whereInput.AddField(new FieldType
                    {
                        Type = inputGraphTypeResolved.GetType(),
                        Name = partName.ToCamelCase(),
                        Description = inputGraphTypeResolved.Description
                    });
                }
            }
        }
    }
}