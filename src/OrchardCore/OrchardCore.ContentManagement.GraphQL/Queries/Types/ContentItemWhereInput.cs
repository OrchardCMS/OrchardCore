using System;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    [GraphQLFieldName("Or", "OR")]
    [GraphQLFieldName("And", "AND")]
    [GraphQLFieldName("Not", "NOT")]
    public class ContentItemWhereInput : InputObjectGraphType
    {
        public class InputOperator
        {
            public string Name { get; set; }

            public string Description { get; set; }
        }

        // Applies to all types
        public static string[] EqualityOperators = { "", "_not" };

        public static string[] NonStringValueComparisonOperators = { "_gt", "_gte", "_lt", "_lte" };

        // Applies to strings
        public static string[] StringComparisonOperators = { "_contains", "_not_contains", "_starts_with", "_not_starts_with", "_ends_with", "_not_ends_with" };

        // Applies to all types
        public static string[] MultiValueComparisonOperators = { "_in", "_not_in" };

        public static InputOperator[] LogicalOperators =
        {
            new InputOperator { Name = "Or", Description = "OR logical operation" },
            new InputOperator { Name = "And", Description = "AND logical operation" },
            new InputOperator { Name = "Not", Description = "NOT logical operation" }
        };

        public ContentItemWhereInput() : this("Article")
        {
        }

        public ContentItemWhereInput(string contentItemName)
        {
            Name = contentItemName + "WhereInput";

            AddScalarFilterField<IdGraphType>("contentItemId", "content item id");
            AddScalarFilterField<IdGraphType>("contentItemVersionId", "the content item version id");
            AddScalarFilterField<StringGraphType>("displayText", "the display text of the content item");
            AddScalarFilterField<DateTimeGraphType>("createdUtc", "the date and time of creation");
            AddScalarFilterField<DateTimeGraphType>("modifiedUtc", "the date and time of modification");
            AddScalarFilterField<DateTimeGraphType>("publishedUtc", "the date and time of publication");
            AddScalarFilterField<StringGraphType>("owner", "the owner of the content item");
            AddScalarFilterField<StringGraphType>("author", "the author of the content item");

            AddLogicalOperators(contentItemName);
        }

        public void AddScalarFilterField<TGraphType>(string fieldName, string description) where TGraphType : IGraphType, new()
        {
            AddEqualityOperators<TGraphType>(fieldName, description);
            AddMultiValueOperators<TGraphType>(fieldName, description);

            var graphType = typeof(TGraphType);
            if (graphType == typeof(StringGraphType))
            {
                AddStringOperators<TGraphType>(fieldName, description);
            }
            else if (graphType == typeof(DateTimeGraphType))
            {
                AddNonStringOperators<TGraphType>(fieldName, description);
            }
        }

        public void AddPartFilterField(IInputObjectGraphType inputType, string fieldName, string description)
        {
            Field(inputType.GetType(), fieldName, description);
        }

        private void AddLogicalOperators(string contentItemName)
        {
            foreach (var filter in LogicalOperators)
            {
                Field<ListGraphType<ContentItemWhereInput>>(filter.Name, filter.Description);
                //field.ResolvedType = new ListGraphType(new ContentItemWhereInput(contentItemName, false));
            }
        }

        private void AddEqualityOperators<TGraphType>(string fieldName, string description) where TGraphType : IGraphType, new()
        {
            AddOperators<TGraphType>(EqualityOperators, fieldName, description);
        }

        private void AddStringOperators<TGraphType>(string fieldName, string description) where TGraphType : IGraphType, new()
        {
            AddOperators<TGraphType>(StringComparisonOperators, fieldName, description);
        }

        private void AddNonStringOperators<TGraphType>(string fieldName, string description) where TGraphType : IGraphType, new()
        {
            AddOperators<TGraphType>(NonStringValueComparisonOperators, fieldName, description);
        }

        private void AddMultiValueOperators<TGraphType>(string fieldName, string description) where TGraphType : IGraphType
        {
            AddOperators<ListGraphType<TGraphType>>(MultiValueComparisonOperators, fieldName, description);
        }

        private void AddOperators<TGraphType>(string[] filters, string fieldName, string description) where TGraphType : IGraphType, new()
        {
            foreach (var comparison in filters)
            {
                Field(typeof(TGraphType), fieldName + comparison, description);
            }
        }
    }
}