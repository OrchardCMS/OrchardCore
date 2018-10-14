using GraphQL.Types;
using OrchardCore.ContentFields.Fields;

namespace OrchardCore.ContentFields.GraphQL
{
    public class DateTimeFieldQueryObjectType : ObjectGraphType<DateTimeField>
    {
        public DateTimeFieldQueryObjectType()
        {
            Name = "DateTimeField";

            Field(x => x.Value.Value);
        }
    }
}
