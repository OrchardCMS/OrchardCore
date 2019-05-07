using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Html.Model;

namespace OrchardCore.Html.GraphQL
{
    public class HtmlBodyQueryObjectType : ObjectGraphType<HtmlBodyPart>
    {
        public HtmlBodyQueryObjectType(IStringLocalizer<HtmlBodyQueryObjectType> T)
        {
            Name = "HtmlBodyPart";
            Description = T["Content stored as HTML."];

            Field(x => x.Html);
        }
    }
}
