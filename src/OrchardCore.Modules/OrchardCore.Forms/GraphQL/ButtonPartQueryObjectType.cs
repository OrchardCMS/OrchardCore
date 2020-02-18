using GraphQL.Types;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.GraphQL
{
    public class ButtonPartQueryObjectType : ObjectGraphType<ButtonPart>
    {
        public ButtonPartQueryObjectType()
        {
            Name = "ButtonPart";

            Field(x => x.Text, nullable: true);
            Field(x => x.Type, nullable: true);
        }
    }
}
