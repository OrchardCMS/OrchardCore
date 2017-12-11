using GraphQL.Types;
using OrchardCore.Title.Model;

namespace OrchardCore.Title.GraphQL
{
    public class TitleInputObjectType : InputObjectGraphType<TitlePart>
    {
        public TitleInputObjectType()
        {
            Name = "TitlePartInput";

            this.AddInputField("title", x => x.Title, true);
        }
    }
}
