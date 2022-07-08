using GraphQL.Types;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.GraphQL
{
    public class HtmlMenuItemQueryObjectType : ObjectGraphType<HtmlMenuItemPart>
    {
        public HtmlMenuItemQueryObjectType()
        {
            Name = "HtmlMenuItemPart";

            Field(x => x.Url, nullable: true);
            Field(x => x.Html, nullable: true);
        }
    }
}
