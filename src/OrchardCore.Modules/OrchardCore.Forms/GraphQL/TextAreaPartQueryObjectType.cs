using GraphQL.Types;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.GraphQL
{
    public class TextAreaPartQueryObjectType : ObjectGraphType<TextAreaPart>
    {
        public TextAreaPartQueryObjectType()
        {
            Name = "TextAreaPart";

            Field(x => x.DefaultValue, nullable: true);
            Field(x => x.Placeholder, nullable: true);
        }
    }
}
