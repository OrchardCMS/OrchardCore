using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Html.Model;

namespace OrchardCore.Html.GraphQL
{
    public class HtmlBodyInputObjectType : QueryArgumentObjectGraphType<HtmlBodyPart>
    {
        public HtmlBodyInputObjectType()
        {
            Name = "HtmlBodyInput";

            AddInputField("Html", x => x.Html, true);
        }
    }
}
