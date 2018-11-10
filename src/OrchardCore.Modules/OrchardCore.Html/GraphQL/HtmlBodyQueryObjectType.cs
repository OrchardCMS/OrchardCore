using GraphQL.Types;
using OrchardCore.Html.Model;

namespace OrchardCore.Html.GraphQL
{
    public class HtmlBodyQueryObjectType : ObjectGraphType<HtmlBodyPart>
    {
        public HtmlBodyQueryObjectType()
        {
            Name = "HtmlBodyPart";

            Field(x => x.Html);
        }
    }
}
