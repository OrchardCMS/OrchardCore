using System;
using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.GraphQL.Types;
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
                    FieldAccessor = field => (bool)field.Content.Value
                }
            },
            {
                nameof(DateField),
                new FieldTypeDescriptor
                {
                    Description = "Date field",
                    FieldType = typeof(DateGraphType),
                    UnderlyingType = typeof(DateField),
                    FieldAccessor = field => (DateTime?)field.Content.Value
                }
            },
            {
                nameof(DateTimeField),
                new FieldTypeDescriptor
                {
                    Description = "Date & time field",
                    FieldType = typeof(DateTimeGraphType),
                    UnderlyingType = typeof(DateTimeField),
                    FieldAccessor = field => (DateTime?)field.Content.Value
                }
            },
            {
                nameof(NumericField),
                new FieldTypeDescriptor
                {
                    Description = "Numeric field",
                    FieldType = typeof(DecimalGraphType),
                    UnderlyingType = typeof(NumericField),
                    FieldAccessor = field => (decimal?)field.Content.Value
                }
            },
            {
                nameof(TextField),
                new FieldTypeDescriptor
                {
                    Description = "Text field",
                    FieldType = typeof(StringGraphType),
                    UnderlyingType = typeof(TextField),
                    FieldAccessor = field => (string)field.Content.Text
                }
            },
            {
                nameof(TimeField),
                new FieldTypeDescriptor
                {
                    Description = "Time field",
                    FieldType = typeof(TimeSpanGraphType),
                    UnderlyingType = typeof(TimeField),
                    FieldAccessor = field => (TimeSpan?)field.Content.Value
                }
            },
            {
                nameof(MultiTextField),
                new FieldTypeDescriptor
                {
                    Description = "Multi text field",
                    FieldType = typeof(ListGraphType<StringGraphType>),
                    UnderlyingType = typeof(MultiTextField),
                    FieldAccessor = field => ((JArray)field.Content.Values)?.ToObject<string[]>()
                }
            }
        };

        public FieldType GetField(ContentPartFieldDefinition field)
        {
            if (!_contentFieldTypeMappings.ContainsKey(field.FieldDefinition.Name))
            {
                return null;
            }

            var fieldDescriptor = _contentFieldTypeMappings[field.FieldDefinition.Name];
            return new FieldType
            {
                Name = field.Name,
                Description = fieldDescriptor.Description,
                Type = fieldDescriptor.FieldType,
                Resolver = new FuncFieldResolver<ContentElement, object>(context =>
                {
                    // Check if part has been collapsed by trying to get the parent part.
                    var contentPart = context.Source.Get(typeof(ContentPart), field.PartDefinition.Name);

                    // Part is not collapsed, access field directly.
                    contentPart ??= context.Source;

                    var contentField = contentPart?.Get(fieldDescriptor.UnderlyingType, field.Name);

                    contentField ??= context.Source.Get(fieldDescriptor.UnderlyingType, field.Name);

                    return contentField == null ? null : fieldDescriptor.FieldAccessor(contentField);
                }),
            };
        }

        private class FieldTypeDescriptor
        {
            public string Description { get; set; }
            public Type FieldType { get; set; }
            public Type UnderlyingType { get; set; }
            public Func<ContentElement, object> FieldAccessor { get; set; }
        }
    }
}
