using System.Linq;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public class DynamicContentTypeBuilder : IContentTypeBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GraphQLContentOptions _contentOptions;

        public DynamicContentTypeBuilder(IHttpContextAccessor httpContextAccessor,
            IOptions<GraphQLContentOptions> contentOptionsAccessor,
            IStringLocalizer<DynamicContentTypeBuilder> localizer)
        {
            _httpContextAccessor = httpContextAccessor;
            _contentOptions = contentOptionsAccessor.Value;

            T = localizer;
        }

        public IStringLocalizer T { get; }

        public void Build(FieldType contentQuery, ContentTypeDefinition contentTypeDefinition, ContentItemType contentItemType)
        {
            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;
            var contentFieldProviders = serviceProvider.GetServices<IContentFieldProvider>().ToList();

            foreach (var part in contentTypeDefinition.Parts)
            {
                var partName = part.Name;

                // Check if another builder has already added a field for this part.
                if (contentItemType.HasField(partName)) continue;

                // This builder only handles parts with fields.
                if (!part.PartDefinition.Fields.Any()) continue;

                if (_contentOptions.ShouldSkip(part)) continue;

                if (_contentOptions.ShouldCollapse(part)) {
                    foreach (var field in part.PartDefinition.Fields)
                    {
                        foreach (var fieldProvider in contentFieldProviders)
                        {
                            var fieldType = fieldProvider.GetField(field);

                            if (fieldType != null)
                            {
                                if (_contentOptions.ShouldSkip(fieldType.Type, fieldType.Name)) continue;

                                contentItemType.AddField(fieldType);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    var field = contentItemType.Field(
                        typeof(DynamicPartGraphType),
                        partName.ToFieldName(),
                        description: T["Represents a {0}.", part.PartDefinition.Name],
                        resolve: context =>
                        {
                            var nameToResolve = partName;
                            var typeToResolve = context.ReturnType.GetType().BaseType.GetGenericArguments().First();

                            return context.Source.Get(typeToResolve, nameToResolve);
                        });

                    field.ResolvedType = new DynamicPartGraphType(_httpContextAccessor, part);
                }
            }
        }
    }
}
