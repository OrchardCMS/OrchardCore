using GraphQL.Types;
using OrchardCore.ContentFields.Fields;

namespace OrchardCore.ContentFields.GraphQL
{
    public class DateFieldQueryObjectType : ObjectGraphType<DateField>
    {
        public DateFieldQueryObjectType()
        {
            Name = "DateField";

            Field(x => x.Value);
        }
    }
}
