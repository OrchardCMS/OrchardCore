using System.Collections.Generic;

namespace OrchardCore.Email;

public class EmailDeliveryServiceKeys : List<object>
{
    public EmailDeliveryServiceKeys(IEnumerable<object> collection) : base(collection)
    {
    }
}
