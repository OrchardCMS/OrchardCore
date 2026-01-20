using GraphQL.Types;
using Microsoft.Extensions.Options;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Options;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
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

            AddFilterField<IdGraphType>("contentItemId", "content item id");
            AddFilterField<IdGraphType>("contentItemVersionId", "the content item version id");
            AddFilterField<StringGraphType>("displayText", "the display text of the content item");
            AddFilterField<DateTimeGraphType>("createdUtc", "the date and time of creation");
            AddFilterField<DateTimeGraphType>("modifiedUtc", "the date and time of modification");
            AddFilterField<DateTimeGraphType>("publishedUtc", "the date and time of publication");
            AddFilterField<StringGraphType>("owner", "the owner of the content item");
            AddFilterField<StringGraphType>("author", "the author of the content item");

            var whereInputType = new ListGraphType(this);
            Field<ListGraphType<ContentItemWhereInput>>("Or", "OR logical operation").ResolvedType = whereInputType;
            Field<ListGraphType<ContentItemWhereInput>>("And", "AND logical operation").ResolvedType = whereInputType;
            Field<ListGraphType<ContentItemWhereInput>>("Not", "NOT logical operation").ResolvedType = whereInputType;
        }

        private void AddFilterField<T>(string name, string description)
        {
            if (!_optionsAccessor.Value.ShouldSkip(typeof(ContentItemType), name))
            {
                AddScalarFilterFields<T>(name, description);
            }
        }
    }
}
