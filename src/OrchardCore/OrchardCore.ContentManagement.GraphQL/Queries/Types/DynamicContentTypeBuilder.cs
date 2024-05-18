using System.Collections.Generic;
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
        // If registered, then fields input should be exposed, otherwise ignored.
        public class FieldsInputMarker
        {
        }

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GraphQLContentOptions _contentOptions;
        protected readonly IStringLocalizer S;
        private readonly Dictionary<string, FieldType> _dynamicPartFields;

        public DynamicContentTypeBuilder(IHttpContextAccessor httpContextAccessor,
            IOptions<GraphQLContentOptions> contentOptionsAccessor,
            IStringLocalizer<DynamicContentTypeBuilder> localizer)
        {
            _httpContextAccessor = httpContextAccessor;
            _contentOptions = contentOptionsAccessor.Value;
            _dynamicPartFields = [];

            S = localizer;
        }

        public void Build(FieldType contentQuery, ContentTypeDefinition contentTypeDefinition, ContentItemType contentItemType)
        {
            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;
            var contentFieldProviders = serviceProvider.GetServices<IContentFieldProvider>().ToList();
            var fieldsInputMarker = serviceProvider.GetService<FieldsInputMarker>();

            var whereInputType = (ContentItemWhereInput)contentQuery.Arguments?.FirstOrDefault(x => x.Name == "where")?.ResolvedType;

            if (_contentOptions.ShouldHide(contentTypeDefinition))
            {
                return;
            }

            foreach (var part in contentTypeDefinition.Parts)
            {
                var partName = part.Name;

                // This builder only handles parts with fields.
                if (!part.PartDefinition.Fields.Any())
                {
                    continue;
                }

                if (_contentOptions.ShouldSkip(part))
                {
                    continue;
                }

                if (!part.PartDefinition.Fields.Any(field => contentFieldProviders.Any(fieldProvider => fieldProvider.HasField(field))))
                {
                    continue;
                }

                if (_contentOptions.ShouldCollapse(part))
                {
                    foreach (var field in part.PartDefinition.Fields)
                    {
                        foreach (var fieldProvider in contentFieldProviders)
                        {
                            var customFieldName = GraphQLContentOptions.GetFieldName(part, part.Name, field.Name);

                            var fieldType = fieldProvider.GetField(field, part.Name, customFieldName);

                            if (fieldType != null)
                            {
                                if (_contentOptions.ShouldSkip(fieldType.Type, fieldType.Name))
                                {
                                    continue;
                                }

                                contentItemType.AddField(fieldType);

                                if (whereInputType != null && fieldsInputMarker != null && fieldProvider.HasFieldIndex(field))
                                {
                                    whereInputType.AddFilterField(fieldType.Type, fieldType.Name, fieldType.Description);
                                }
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // Check if another builder has already added a field for this part.
                    var existingField = contentItemType.GetField(partName.ToFieldName());
                    if (existingField != null)
                    {
                        // Add content field types.
                        foreach (var field in part.PartDefinition.Fields)
                        {
                            foreach (var fieldProvider in contentFieldProviders)
                            {
                                var contentFieldType = fieldProvider.GetField(field, part.Name);

                                if (contentFieldType != null && !contentItemType.HasField(contentFieldType.Name))
                                {
                                    contentItemType.AddField(contentFieldType);

                                    if (whereInputType != null && fieldsInputMarker != null && fieldProvider.HasFieldIndex(field))
                                    {
                                        whereInputType.AddFilterField(contentFieldType.Type, contentFieldType.Name, contentFieldType.Description);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    else if (_dynamicPartFields.TryGetValue(partName, out var fieldType))
                    {
                        contentItemType.AddField(fieldType);

                        if (whereInputType != null && fieldsInputMarker != null)
                        {
                            whereInputType?.AddFilterField(fieldType.Type, fieldType.Name, fieldType.Description);
                        }
                    }
                    else
                    {
                        var field = contentItemType
                            .Field<DynamicPartGraphType>(partName.ToFieldName())
                            .Description(S["Represents a {0}.", part.PartDefinition.Name])
                            .Resolve(context =>
                            {
                                var nameToResolve = partName;
                                var typeToResolve = context.FieldDefinition.ResolvedType.GetType().BaseType.GetGenericArguments().First();

                                return context.Source.Get(typeToResolve, nameToResolve);
                            });

                        field.Type(new DynamicPartGraphType(_httpContextAccessor, part));

                        if (whereInputType != null &&
                            fieldsInputMarker != null &&
                            part.PartDefinition.Fields.Any(field => contentFieldProviders.Any(cfp => cfp.HasFieldIndex(field))))
                        {
                            var inputField = whereInputType
                                .Field<DynamicPartInputGraphType>(partName.ToFieldName())
                                .Description(S["Represents a {0} input.", part.PartDefinition.Name]);

                            inputField.Type(new DynamicPartInputGraphType(_httpContextAccessor, part));
                        }

                        _dynamicPartFields[partName] = field.FieldType;
                    }
                }
            }
        }

        public void Clear()
        {
            _dynamicPartFields.Clear();
        }
    }
}
