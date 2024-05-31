using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public abstract class DynamicContentTypeBuilder : IContentTypeBuilder
    {
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly IEnumerable<IContentFieldProvider> _contentFieldProviders;
        protected readonly GraphQLContentOptions _contentOptions;
        protected readonly IStringLocalizer S;
        private readonly Dictionary<string, FieldType> _dynamicPartFields;

        public DynamicContentTypeBuilder(IHttpContextAccessor httpContextAccessor,
            IEnumerable<IContentFieldProvider> contentFieldProviders,
            IOptions<GraphQLContentOptions> contentOptionsAccessor,
            IStringLocalizer<DynamicContentTypeBuilder> localizer)
        {
            _httpContextAccessor = httpContextAccessor;
            _contentFieldProviders = contentFieldProviders;
            _contentOptions = contentOptionsAccessor.Value;
            _dynamicPartFields = [];

            S = localizer;
        }

        public abstract void Build(ISchema schema, FieldType contentQuery, ContentTypeDefinition contentTypeDefinition, ContentItemType contentItemType);

        public void BuildInternal(ISchema schema, FieldType contentQuery, ContentTypeDefinition contentTypeDefinition, ComplexGraphType<ContentItem> graphType)
        {
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

                if (!part.PartDefinition.Fields.Any(field => _contentFieldProviders.Any(fieldProvider => fieldProvider.HasField(schema, field))))
                {
                    continue;
                }

                if (_contentOptions.ShouldCollapse(part))
                {
                    foreach (var field in part.PartDefinition.Fields)
                    {
                        foreach (var fieldProvider in _contentFieldProviders)
                        {
                            var customFieldName = GraphQLContentOptions.GetFieldName(part, part.Name, field.Name);

                            var fieldType = fieldProvider.GetField(schema, field, part.Name, customFieldName);

                            if (fieldType != null)
                            {
                                if (_contentOptions.ShouldSkip(fieldType.Type, fieldType.Name))
                                {
                                    continue;
                                }

                                if (graphType is WhereInputObjectGraphType<ContentItem> whereGraphType)
                                {
                                    if (fieldProvider.HasFieldIndex(field))
                                    {
                                        whereGraphType.AddScalarFilterFields(fieldType);
                                    }
                                }
                                else
                                {
                                    graphType.AddField(fieldType);
                                }

                                break;
                            }
                        }
                    }
                }
                else
                {
                    // Check if another builder has already added a field for this part.
                    var existingField = graphType.GetField(partName.ToFieldName());
                    if (existingField != null)
                    {
                        // Add content field types.
                        foreach (var field in part.PartDefinition.Fields)
                        {
                            foreach (var fieldProvider in _contentFieldProviders)
                            {
                                var contentFieldType = fieldProvider.GetField(schema, field, part.Name);

                                if (contentFieldType != null && !graphType.HasField(contentFieldType.Name))
                                {
                                    if (graphType is WhereInputObjectGraphType<ContentItem> whereGraphType)
                                    {
                                        if (fieldProvider.HasFieldIndex(field))
                                        {
                                            whereGraphType.AddScalarFilterFields(contentFieldType);
                                        }
                                    }
                                    else
                                    {
                                        graphType.AddField(contentFieldType);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    else if (_dynamicPartFields.TryGetValue(partName, out var fieldType))
                    {
                        if (graphType is WhereInputObjectGraphType<ContentItem> whereGraphType)
                        {
                            whereGraphType.AddScalarFilterFields(fieldType);
                        }
                        else
                        {
                            graphType.AddField(fieldType);
                        }
                    }
                    else
                    {
                        if (graphType is WhereInputObjectGraphType<ContentItem> whereGraphType)
                        {
                            if (part.PartDefinition.Fields.Any(field => _contentFieldProviders.Any(cfp => cfp.HasFieldIndex(field))))
                            {
                                var inputField = whereGraphType
                                    .Field<DynamicPartInputGraphType>(partName.ToFieldName())
                                    .Description(S["Represents a {0} input.", part.PartDefinition.Name]);

                                inputField.Type(new DynamicPartInputGraphType(_httpContextAccessor, part));
                                _dynamicPartFields[partName] = inputField.FieldType;
                            }
                        }
                        else
                        {
                            var field = graphType
                                .Field<DynamicPartGraphType>(partName.ToFieldName())
                                .Description(S["Represents a {0}.", part.PartDefinition.Name]);

                        field.Type(new DynamicPartGraphType(part));
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
