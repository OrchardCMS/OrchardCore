using System.Linq;
using GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Options;

namespace OrchardCore.ContentManagement.GraphQL
{
    public static class GraphQLContentOptionsExtensions
    {
        public static GraphQLContentOptions SetPartAsCollapsed<TContentPart>(
            this GraphQLContentOptions options)
            where TContentPart : ContentPart
        {
            options.PartOptions = options.PartOptions.Union(new[] { new GraphQLContentPartOption {
                Name = typeof(TContentPart).Name,
                Collapse = true
            } });

            return options;
        }

        public static GraphQLContentOptions SetFieldAsIgnored<TObjectGraphType>(
            this GraphQLContentOptions options,
            string fieldName)
            where TObjectGraphType : IObjectGraphType
        {
            options.IgnoredFields = options.IgnoredFields.Union(
                                new[] {
                        new GraphQLField {
                            FieldName = fieldName,
                            FieldType = typeof(TObjectGraphType)
                        }});

            return options;
        }

    }
}
