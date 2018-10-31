using System;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public enum ScalarType
    {
        Identifier,
        String,
        DateTime
    }

    [GraphQLFieldName("Or", "OR")]
    [GraphQLFieldName("And", "AND")]
    public class ContentItemWhereInput : InputObjectGraphType
    {
        // Applies to all types
        public static string[] EqualityOperators = { "", "_not" };

        public static string[] NonStringValueComparisonOperators = { "_gt", "_gte", "_lt", "_lte" };

        // Applies to strings
        public static string[] StringComparisonOperators = { "_contains", "_not_contains", "_starts_with", "_not_starts_with", "_ends_with", "_not_ends_with" };

        // Applies to all types
        public static string[] MultiValueComparisonOperators = { "_in", "_not_in" };

        public ContentItemWhereInput(string contentItemName, bool addLogicalOperators = true)
        {
            Name = contentItemName + "WhereInput";

            AddScalarFilterField<StringGraphType>("contentItemId", "content item id", ScalarType.Identifier);
            AddScalarFilterField<StringGraphType>("contentItemVersionId", "the content item version id", ScalarType.Identifier);
            AddScalarFilterField<StringGraphType>("displayText", "the display text of the content item", ScalarType.String);
            AddScalarFilterField<DateTimeGraphType>("createdUtc", "the date and time of creation", ScalarType.DateTime);
            AddScalarFilterField<DateTimeGraphType>("modifiedUtc", "the date and time of modification", ScalarType.DateTime);
            AddScalarFilterField<DateTimeGraphType>("publishedUtc", "the date and time of publication", ScalarType.DateTime);
            AddScalarFilterField<StringGraphType>("owner", "the owner of the content item", ScalarType.Identifier);
            AddScalarFilterField<StringGraphType>("author", "the author of the content item", ScalarType.Identifier);

            if (addLogicalOperators)
            {
                var orField = Field(typeof(ContentItemWhereInput), "Or", "OR logical operation");
                orField.ResolvedType = new ContentItemWhereInput(contentItemName, false);

                var andField = Field(typeof(ContentItemWhereInput), "And", "AND logical operation");
                andField.ResolvedType = new ContentItemWhereInput(contentItemName, false);
            }
        }

        private void AddScalarFilterField<TGraphType>(string fieldName, string description, ScalarType type) where TGraphType : IGraphType
        {
            switch (type)
            {
                case ScalarType.Identifier:
                    {
                        AddEqualityOperators<TGraphType>(fieldName, description);
                        AddMultiValueOperators<TGraphType>(fieldName, description);
                        break;
                    }
                case ScalarType.String:
                    {
                        AddEqualityOperators<TGraphType>(fieldName, description);
                        AddStringOperators<TGraphType>(fieldName, description);
                        AddMultiValueOperators<TGraphType>(fieldName, description);
                        break;
                    } 
                case ScalarType.DateTime:
                    {
                        AddEqualityOperators<TGraphType>(fieldName, description);
                        AddNonStringOperators<TGraphType>(fieldName, description);
                        AddMultiValueOperators<TGraphType>(fieldName, description);
                        break;
                    }               
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            } 
        }

        private void AddEqualityOperators<TGraphType>(string fieldName, string description) where TGraphType : IGraphType
        {
            foreach (var comparison in EqualityOperators)
            {
                Field<TGraphType>(fieldName + comparison, description);
            }
        }

        private void AddStringOperators<TGraphType>(string fieldName, string description) where TGraphType : IGraphType
        {
            foreach (var comparison in StringComparisonOperators)
            {
                Field<TGraphType>(fieldName + comparison, description);
            }
        }

        private void AddNonStringOperators<TGraphType>(string fieldName, string description) where TGraphType : IGraphType
        {
            foreach (var comparison in NonStringValueComparisonOperators)
            {
                Field<TGraphType>(fieldName + comparison, description);
            }
        }

        private void AddMultiValueOperators<TGraphType>(string fieldName, string description) where TGraphType : IGraphType
        {
            foreach (var comparison in MultiValueComparisonOperators)
            {
                Field<ListGraphType<TGraphType>>(fieldName + comparison, description);
            }
        }
    }
}