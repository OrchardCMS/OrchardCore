using GraphQL.Types;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.GraphQL;

public class LinkMenuItemQueryObjectType : ObjectGraphType<LinkMenuItemPart>
{
    public LinkMenuItemQueryObjectType()
    {
        Name = "LinkMenuItemPart";

        Field(x => x.Url, nullable: true);
    }
}
