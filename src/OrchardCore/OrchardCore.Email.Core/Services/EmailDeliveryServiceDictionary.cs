using System.Collections.Generic;

namespace OrchardCore.Email.Services;

public class EmailDeliveryServiceDictionary : Dictionary<string, IEmailDeliveryService>
{
}
