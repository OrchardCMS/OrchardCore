using GraphQL.Types;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Options;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

[GraphQLFieldName("Or", "OR")]
[GraphQLFieldName("And", "AND")]
[GraphQLFieldName("Not", "NOT")]
public sealed class ContentItemWhereInput : WhereInputObjectGraphType
{
    private readonly IOptions<GraphQLContentOptions> _optionsAccessor;

    public ContentItemWhereInput(string contentItemName, IOptions<GraphQLContentOptions> optionsAccessor, IStringLocalizer<ContentItemWhereInput> stringLocalizer)
        : base(stringLocalizer)
    {
        _optionsAccessor = optionsAccessor;

        Name = $"{contentItemName}WhereInput";

        Description = S["the {0} content item filters", contentItemName];

        AddScalarFilterFields<IdGraphType>("contentItemId", S["content item id"]);
        AddScalarFilterFields<IdGraphType>("contentItemVersionId", S["the content item version id"]);
        AddScalarFilterFields<StringGraphType>("displayText", S["the display text of the content item"]);
        AddScalarFilterFields<DateTimeGraphType>("createdUtc", S["the date and time of creation"]);
        AddScalarFilterFields<DateTimeGraphType>("modifiedUtc", S["the date and time of modification"]);
        AddScalarFilterFields<DateTimeGraphType>("publishedUtc", S["the date and time of publication"]);
        AddScalarFilterFields<StringGraphType>("owner", S["the owner of the content item"]);
        AddScalarFilterFields<StringGraphType>("author", S["the author of the content item"]);

        var whereInputType = new ListGraphType(this);

        Field<ListGraphType<ContentItemWhereInput>>("Or")
            .Description(S["OR logical operation"])
            .Type(whereInputType);

        Field<ListGraphType<ContentItemWhereInput>>("And")
            .Description(S["AND logical operation"])
            .Type(whereInputType);

        Field<ListGraphType<ContentItemWhereInput>>("Not")
            .Description(S["NOT logical operation"])
            .Type(whereInputType);
    }

    public override void AddScalarFilterFields(Type graphType, string fieldName, string description)
    {
        if (!_optionsAccessor.Value.ShouldSkip(typeof(ContentItemType), fieldName))
        {
            base.AddScalarFilterFields(graphType, fieldName, description);
        }
    }
}
