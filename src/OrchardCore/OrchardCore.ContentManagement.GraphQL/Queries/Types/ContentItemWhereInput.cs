using System;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public class ContentItemWhereInputModel
    {
        public PublicationStatusEnum Status { get; set; }

        public string SingleStringValue { get; set; }

        public string[] MultipleStringValue { get; set; }

        public DateTime SingleDateTimeValue { get; set; }

        public DateTime[] MultipleDateTimeValue { get; set; }

        public ContentItemWhereInputModel[] Or { get; set; }

        public ContentItemWhereInputModel[] And { get; set; }
    }

    [GraphQLFieldName("Or", "OR")]
    [GraphQLFieldName("And", "AND")]
    public class ContentItemWhereInput : InputObjectGraphType<ContentItemWhereInputModel>

    {
        // Applies to all types
        public static string[] EqualityOperators = new[] { "", "_not" };

        public static string[] NonStringValueComparisonOperators = new[] { "_gt", "_gte", "_lt", "_lte" };

        // Applies to strings
        public static string[] StringComparisonOperators = new[] { "_contains", "_not_contains", "_starts_with", "_not_starts_with", "_ends_with", "_not_ends_with" };

        // Applies to all types
        public static string[] MultiValueComparisonOperators = new[] { "_in", "_not_in" };

        public ContentItemWhereInput(string contentItemName)
        {
            Name = contentItemName + "WhereInput";

            // TODO: Ideally we should return a custom FieldType that contains the name of the property and the YesSql Expression to filter
            foreach (var comparison in EqualityOperators)
            {
                Field("contentItemId" + comparison, x => x.SingleStringValue, nullable: true).Description("content item id");
                Field("contentItemVersionId" + comparison, x => x.SingleStringValue, nullable: true).Description("the content item version id");
                Field("displayText" + comparison, x => x.SingleStringValue, nullable: true).Description("the display text of the content item");
                Field("createdUtc" + comparison, x => x.SingleDateTimeValue, nullable: true).Description("the date and time of creation");
                Field("modifiedUtc" + comparison, x => x.SingleDateTimeValue, nullable: true).Description("the date and time of modification");
                Field("publishedUtc" + comparison, x => x.SingleDateTimeValue, nullable: true).Description("the date and time of publication");
                Field("owner" + comparison, x => x.SingleStringValue, nullable: true).Description("the owner of the content item");
                Field("author" + comparison, x => x.SingleStringValue, nullable: true).Description("the author of the content item");
            }

            foreach (var comparison in NonStringValueComparisonOperators)
            {
                Field("createdUtc" + comparison, x => x.SingleDateTimeValue, nullable: true).Description("the date and time of creation");
                Field("modifiedUtc" + comparison, x => x.SingleDateTimeValue, nullable: true).Description("the date and time of modification");
                Field("publishedUtc" + comparison, x => x.SingleDateTimeValue, nullable: true).Description("the date and time of publication");
            }

            foreach (var comparison in StringComparisonOperators)
            {
                Field("displayText" + comparison, x => x.SingleStringValue, nullable: true).Description("the display text of the content item");
            }

            foreach (var comparison in MultiValueComparisonOperators)
            {
                Field("contentItemId" + comparison, x => x.MultipleStringValue, nullable: true).Description("content item id");
                Field("displayText" + comparison, x => x.MultipleStringValue, nullable: true).Description("the display text of the content item");
                Field("createdUtc" + comparison, x => x.MultipleDateTimeValue, nullable: true).Description("the date and time of creation");
                Field("modifiedUtc" + comparison, x => x.MultipleDateTimeValue, nullable: true).Description("the date and time of modification");
                Field("publishedUtc" + comparison, x => x.MultipleDateTimeValue, nullable: true).Description("the date and time of publication");
                Field("contentItemVersionId" + comparison, x => x.MultipleStringValue, nullable: true).Description("the content item version id");
            }

            Field(x => x.Or, nullable: true, typeof(ContentItemWhereInput)).Description("OR logical operation");
            Field(x => x.And, nullable: true, typeof(ContentItemWhereInput)).Description("AND logical operation");
        }
    }
}