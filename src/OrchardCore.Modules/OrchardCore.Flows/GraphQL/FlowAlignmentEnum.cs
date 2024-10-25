using GraphQL.Types;

namespace OrchardCore.Flows.GraphQL;

public class FlowAlignmentEnum : EnumerationGraphType
{
    public FlowAlignmentEnum()
    {
        Name = "FlowAlignment";

        Description = "The widget alignment.";
        Add("Left", 0, "Left alignment.");
        Add("Center", 1, "Center alignment.");
        Add("Right", 2, "Right alignment.");
        Add("Justify", 3, "Justify alignment.");
    }
}
