using GraphQL.Types;
using OrchardCore.Html.Models;

namespace OrchardCore.Html.GraphQL;

public class HtmlMenuItemQueryObjectType : ObjectGraphType<HtmlMenuItemPart>
{
    public HtmlMenuItemQueryObjectType()
    {
        Name = "HtmlMenuItemPart";

        Field(x => x.Url, nullable: true);
        Field(x => x.Target, nullable: true);
        Field(x => x.Html, nullable: true);
    }
}
