using GraphQL.Types;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Flows.GraphQL;

public class FlowAlignmentEnum : EnumerationGraphType
{
    public FlowAlignmentEnum(IStringLocalizer<FlowAlignmentEnum> S)
    {
        Name = "FlowAlignment";

        Description = S["The widget alignment."];
        Add("Left", 0, S["Left alignment."]);
        Add("Center", 1, S["Center alignment."]);
        Add("Right", 2, S["Right alignment."]);
        Add("Justify", 3, S["Justify alignment."]);
    }
}
