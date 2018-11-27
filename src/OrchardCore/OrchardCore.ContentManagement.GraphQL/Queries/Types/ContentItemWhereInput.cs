using GraphQL.Types;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Queries;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    [GraphQLFieldName("Or", "OR")]
    [GraphQLFieldName("And", "AND")]
    [GraphQLFieldName("Not", "NOT")]
    public class ContentItemWhereInput : WhereInputObjectGraphType
    {
        public ContentItemWhereInput(string contentItemName)
        {
            Name =  $"{contentItemName}Where";
            
            Description = $"the {contentItemName} content item filters";

            AddScalarFilterFields<IdGraphType>("contentItemId", "content item id");
            AddScalarFilterFields<IdGraphType>("contentItemVersionId", "the content item version id");
            AddScalarFilterFields<StringGraphType>("displayText", "the display text of the content item");
            AddScalarFilterFields<DateTimeGraphType>("createdUtc", "the date and time of creation");
            AddScalarFilterFields<DateTimeGraphType>("modifiedUtc", "the date and time of modification");
            AddScalarFilterFields<DateTimeGraphType>("publishedUtc", "the date and time of publication");
            AddScalarFilterFields<StringGraphType>("owner", "the owner of the content item");
            AddScalarFilterFields<StringGraphType>("author", "the author of the content item");

            var whereInputType = new ListGraphType(this);
            Field<ListGraphType<ContentItemWhereInput>>("Or", "OR logical operation").ResolvedType = whereInputType;
            Field<ListGraphType<ContentItemWhereInput>>("And", "AND logical operation").ResolvedType = whereInputType;
            Field<ListGraphType<ContentItemWhereInput>>("Not", "NOT logical operation").ResolvedType = whereInputType;
        }
    }
}