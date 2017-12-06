using GraphQL.Types;
using OrchardCore.Title.Model;

namespace OrchardCore.Title.GraphQL
{
    public class TitleInputObjectType : InputObjectGraphType<TitlePart>
    {
        public TitleInputObjectType()
        {
            Name = "TitlePartInput";

            Field(x => x.Title, false);
        }
    }
}
