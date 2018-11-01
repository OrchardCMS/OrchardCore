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

            var fields = new[] {
                new { Type = typeof(IdGraphType),  Name = "contentItemId",  Description = "content item id" },
                new { Type = typeof(IdGraphType),  Name = "contentItemVersionId",  Description = "the content item version id" },
                new { Type = typeof(StringGraphType),  Name = "displayText",  Description = "the display text of the content item" },
                new { Type = typeof(DateTimeGraphType),  Name = "createdUtc",  Description = "the date and time of creation" },
                new { Type = typeof(DateTimeGraphType),  Name = "modifiedUtc",  Description = "the date and time of modification" },
                new { Type = typeof(DateTimeGraphType),  Name = "publishedUtc",  Description = "the date and time of publication" },
                new { Type = typeof(StringGraphType),  Name = "owner",  Description = "the owner of the content item" },
                new { Type = typeof(StringGraphType),  Name = "author",  Description = "the author of the content item" }
            };

            foreach (var field in fields)
            {
                this.AddScalarFilterFields(field.Type, field.Name, field.Description);
            }

            AddLogicalOperators(contentItemName);
        }

        public void AddLogicalOperators(string contentItemName)
        {
            foreach (var filter in LogicalOperators)
            {
                AddField(new FieldType
                {
                    Type = typeof(ListGraphType<ContentItemWhereInput>),
                    Name = filter.Name,
                    Description = filter.Description
                });
            }
        }
    }
}