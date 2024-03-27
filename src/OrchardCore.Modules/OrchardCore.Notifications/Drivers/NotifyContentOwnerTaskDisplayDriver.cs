using System;
using OrchardCore.Notifications.Activities;

namespace OrchardCore.Notifications.Drivers;

public class NotifyContentOwnerTaskDisplayDriver : NotifyUserTaskActivityDisplayDriver<NotifyContentOwnerTask>
{
    public NotifyContentOwnerTaskDisplayDriver(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
}
