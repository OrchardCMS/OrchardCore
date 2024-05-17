using System;
using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Queries.Types;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.GraphQL.Types;
using OrchardCore.ContentFields.Indexing.SQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.GraphQL.Fields
{
    public class ContentFieldsProvider : IContentFieldProvider
    {
        private static readonly Dictionary<string, FieldTypeDescriptor> _contentFieldTypeMappings = new()
        {
            {
                nameof(BooleanField),
                new FieldTypeDescriptor
                {
                    Description = "Boolean field",
                    FieldType = typeof(BooleanGraphType),
                    UnderlyingType = typeof(BooleanField),
                    FieldAccessor = field => ((BooleanField)field).Value,
                    IndexDescriptor = new FieldTypeIndexDescriptor
                    {
                        IndexType = typeof(BooleanFieldIndex),
                        Index = nameof(BooleanFieldIndex.Boolean)
                    }
                }
            },
            {
                nameof(DateField),
                new FieldTypeDescriptor
                {
                    Description = "Date field",
                    FieldType = typeof(DateGraphType),
                    UnderlyingType = typeof(DateField),
                    FieldAccessor = field => ((DateField)field).Value,
                    IndexDescriptor = new FieldTypeIndexDescriptor
                    {
                        IndexType = typeof(DateFieldIndex),
                        Index = nameof(DateFieldIndex.Date)
                    }
                }
            },
            {
                nameof(DateTimeField),
                new FieldTypeDescriptor
                {
                    Description = "Date & time field",
                    FieldType = typeof(DateTimeGraphType),
                    UnderlyingType = typeof(DateTimeField),
                    FieldAccessor = field => ((DateTimeField)field).Value,
                    IndexDescriptor = new FieldTypeIndexDescriptor
                    {
                        IndexType = typeof(DateTimeFieldIndex),
                        Index = nameof(DateTimeFieldIndex.DateTime)
                    }
                }
            },
            {
                nameof(NumericField),
                new FieldTypeDescriptor
                {
                    Description = "Numeric field",
                    FieldType = typeof(DecimalGraphType),
                    UnderlyingType = typeof(NumericField),
                    FieldAccessor = field => ((NumericField)field).Value,
                    IndexDescriptor = new FieldTypeIndexDescriptor
                    {
                        IndexType = typeof(NumericFieldIndex),
                        Index = nameof(NumericFieldIndex.Numeric)
                    }
                }
            },
            {
                nameof(TextField),
                new FieldTypeDescriptor
                {
                    Description = "Text field",
                    FieldType = typeof(StringGraphType),
                    UnderlyingType = typeof(TextField),
                    FieldAccessor = field => ((TextField)field).Text,
                    IndexDescriptor = new FieldTypeIndexDescriptor
                    {
                        IndexType = typeof(TextFieldIndex),
                        Index = nameof(TextFieldIndex.Text)
                    }
                }
            },
            {
                nameof(TimeField),
                new FieldTypeDescriptor
                {
                    Description = "Time field",
                    FieldType = typeof(TimeSpanGraphType),
                    UnderlyingType = typeof(TimeField),
                    FieldAccessor = field => ((TimeField)field).Value,
                    IndexDescriptor = new FieldTypeIndexDescriptor
                    {
                        IndexType = typeof(TimeFieldIndex),
                        Index = nameof(TimeFieldIndex.Time)
                    }
                }
            },
            {
                nameof(MultiTextField),
                new FieldTypeDescriptor
                {
                    Description = "Multi text field",
                    FieldType = typeof(ListGraphType<StringGraphType>),
                    UnderlyingType = typeof(MultiTextField),
                    FieldAccessor = field => ((MultiTextField)field).Values,
                }
            }
        };

        public FieldType GetField(ContentPartFieldDefinition field, string namedPartTechnicalName, string customFieldName)
        {
            if (!_contentFieldTypeMappings.TryGetValue(field.FieldDefinition.Name, out var value))
            {
                return null;
            }

            var fieldDescriptor = value;
            return new FieldType
            {
                Name = customFieldName ?? field.Name,
                Description = fieldDescriptor.Description,
                Type = fieldDescriptor.FieldType,
                Resolver = new FuncFieldResolver<ContentElement, object>(context =>
                {
                    // Check if part has been collapsed by trying to get the parent part.
                    ContentElement contentPart = context.Source.Get<ContentPart>(field.PartDefinition.Name);

                    // Part is not collapsed, access field directly.
                    contentPart ??= context.Source;

                    var contentField = contentPart?.Get(fieldDescriptor.UnderlyingType, field.Name);

                    contentField ??= context.Source.Get(fieldDescriptor.UnderlyingType, field.Name);

                    return contentField == null ? null : fieldDescriptor.FieldAccessor(contentField);
                }),
            };
        }

        public bool HasField(ContentPartFieldDefinition field) => _contentFieldTypeMappings.ContainsKey(field.FieldDefinition.Name);

        public (string, Type) GetFieldIndex(ContentPartFieldDefinition field)
        {
            if (!_contentFieldTypeMappings.TryGetValue(field.FieldDefinition.Name, out var fieldDescriptor) || 
                    fieldDescriptor.IndexDescriptor == null)
            {
                return (null, null);
            }

            return (fieldDescriptor.IndexDescriptor.Index, fieldDescriptor.IndexDescriptor.IndexType);
        }

        private sealed class FieldTypeDescriptor
        {
            public string Description { get; set; }
            public Type FieldType { get; set; }
            public Type UnderlyingType { get; set; }
            public Func<ContentElement, object> FieldAccessor { get; set; }
            public FieldTypeIndexDescriptor IndexDescriptor { get; set; }
        }

        private sealed class FieldTypeIndexDescriptor
        {
            public Type IndexType { get; set; }
            public string Index { get; set; }
        }
    }
}
