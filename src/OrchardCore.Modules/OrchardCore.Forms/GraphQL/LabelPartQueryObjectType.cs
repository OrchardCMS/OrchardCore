using GraphQL.Types;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.GraphQL;

public class LabelPartQueryObjectType : ObjectGraphType<LabelPart>
{
    public LabelPartQueryObjectType()
    {
        Name = "LabelPart";

        Field(x => x.For, nullable: true);
        Field("value", context => context.ContentItem.DisplayText);
    }
}
