using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public class ContentItemOrderByInput : InputObjectGraphType
{
    public ContentItemOrderByInput(string contentItemName)
    {
        Name = contentItemName + "OrderByInput";

        Field<OrderByDirectionGraphType>("contentItemId");
        Field<OrderByDirectionGraphType>("contentItemVersionId");
        Field<OrderByDirectionGraphType>("contentType");
        Field<OrderByDirectionGraphType>("displayText");
        Field<OrderByDirectionGraphType>("published");
        Field<OrderByDirectionGraphType>("latest");
        Field<OrderByDirectionGraphType>("createdUtc");
        Field<OrderByDirectionGraphType>("modifiedUtc");
        Field<OrderByDirectionGraphType>("publishedUtc");
        Field<OrderByDirectionGraphType>("owner");
        Field<OrderByDirectionGraphType>("author");
    }
}

public enum OrderByDirection
{
    Ascending,
    Descending
}

public class OrderByDirectionGraphType : EnumerationGraphType
{
    public OrderByDirectionGraphType()
    {
        Name = "OrderByDirection";
        Description = "the order by direction";
        Add("ASC", OrderByDirection.Ascending, "orders content items in ascending order");
        Add("DESC", OrderByDirection.Descending, "orders content items in descending order");
    }
}
