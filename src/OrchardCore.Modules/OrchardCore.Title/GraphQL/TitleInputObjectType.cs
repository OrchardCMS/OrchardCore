using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Title.Model;

namespace OrchardCore.Title.GraphQL
{
    public class TitleInputObjectType : QueryArgumentObjectGraphType<TitlePart>
    {
        public TitleInputObjectType()
        {
            Name = "TitlePartInput";

            AddInputField("title", x => x.Title, true);
        }
    }
}
