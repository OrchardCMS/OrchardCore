using GraphQL.Types;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.GraphQL
{
    public class LinkMenuItemQueryObjectType : ObjectGraphType<LinkMenuItemPart>
    {
        public LinkMenuItemQueryObjectType()
        {
            Name = "LinkMenuItemPart";

            // This code can be removed in a later release.
#pragma warning disable 0618
            Field(x => x.Name, nullable: true).Description("Deprecated. Use displayText.");
#pragma warning restore 0618
            Field(x => x.Url, nullable: true);
        }
    }
}
