using GraphQL.Types;
using Microsoft.Extensions.Options;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Options;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

[GraphQLFieldName("Or", "OR")]
[GraphQLFieldName("And", "AND")]
[GraphQLFieldName("Not", "NOT")]
public class ContentItemWhereInput : WhereInputObjectGraphType
{
    private readonly IOptions<GraphQLContentOptions> _optionsAccessor;

    public ContentItemWhereInput(string contentItemName, IOptions<GraphQLContentOptions> optionsAccessor)
    {
        _optionsAccessor = optionsAccessor;

        Name = $"{contentItemName}WhereInput";

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
        Field<ListGraphType<ContentItemWhereInput>>("Or").Description("OR logical operation").Type(whereInputType);
        Field<ListGraphType<ContentItemWhereInput>>("And").Description("AND logical operation").Type(whereInputType);
        Field<ListGraphType<ContentItemWhereInput>>("Not").Description("NOT logical operation").Type(whereInputType);
    }

    public override void AddScalarFilterFields(Type graphType, string fieldName, string description)
    {
        if (!_optionsAccessor.Value.ShouldSkip(typeof(ContentItemType), fieldName))
        {
            base.AddScalarFilterFields(graphType, fieldName, description);
        }
    }
}
