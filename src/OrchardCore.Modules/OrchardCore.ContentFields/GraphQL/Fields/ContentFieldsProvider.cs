using System.Collections.Frozen;
using GraphQL.Resolvers;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Queries.Types;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Indexing.SQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.GraphQL.Fields;

public class ContentFieldsProvider : IContentFieldProvider
{
    private static readonly FrozenDictionary<string, FieldTypeDescriptor> _contentFieldTypeMappings = new Dictionary<string, FieldTypeDescriptor>()
    {
        {
            nameof(BooleanField),
            new FieldTypeDescriptor
            {
                Description = "Boolean field",
                FieldType = typeof(BooleanGraphType),
                UnderlyingType = typeof(BooleanField),
                FieldAccessor = field => ((BooleanField)field).Value,
                IndexType = typeof(BooleanFieldIndex),
                Index = nameof(BooleanFieldIndex.Boolean)
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
                IndexType = typeof(DateFieldIndex),
                Index = nameof(DateFieldIndex.Date)
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
                IndexType = typeof(DateTimeFieldIndex),
                Index = nameof(DateTimeFieldIndex.DateTime)
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
                IndexType = typeof(NumericFieldIndex),
                Index = nameof(NumericFieldIndex.Numeric)
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
                IndexType = typeof(TextFieldIndex),
                Index = nameof(TextFieldIndex.Text)
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
                IndexType = typeof(TimeFieldIndex),
                Index = nameof(TimeFieldIndex.Time)
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
    }.ToFrozenDictionary();

    public FieldType GetField(ISchema schema, ContentPartFieldDefinition field, string namedPartTechnicalName, string customFieldName)
    {
        if (!_contentFieldTypeMappings.TryGetValue(field.FieldDefinition.Name, out var fieldDescriptor))
        {
            return null;
        }

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

    public bool HasField(ISchema schema, ContentPartFieldDefinition field) => _contentFieldTypeMappings.ContainsKey(field.FieldDefinition.Name);

    public FieldTypeIndexDescriptor GetFieldIndex(ContentPartFieldDefinition field)
    {
        if (!HasFieldIndex(field))
        {
            return null;
        }

        var fieldDescriptor = _contentFieldTypeMappings[field.FieldDefinition.Name];

        return new FieldTypeIndexDescriptor
        {
            Index = fieldDescriptor.Index,
            IndexType = fieldDescriptor.IndexType
        };
    }

    public bool HasFieldIndex(ContentPartFieldDefinition field) =>
        _contentFieldTypeMappings.TryGetValue(field.FieldDefinition.Name, out var fieldTypeDescriptor) &&
        fieldTypeDescriptor.IndexType != null &&
        !string.IsNullOrWhiteSpace(fieldTypeDescriptor.Index);

    private sealed class FieldTypeDescriptor
    {
        public string Description { get; set; }
        public Type FieldType { get; set; }
        public Type UnderlyingType { get; set; }
        public Func<ContentElement, object> FieldAccessor { get; set; }
        public string Index { get; set; }
        public Type IndexType { get; set; }
    }
}
