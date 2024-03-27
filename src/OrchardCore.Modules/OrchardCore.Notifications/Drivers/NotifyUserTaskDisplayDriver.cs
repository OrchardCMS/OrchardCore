using System;
using OrchardCore.Notifications.Activities;

namespace OrchardCore.Notifications.Drivers;

public class NotifyUserTaskDisplayDriver : NotifyUserTaskActivityDisplayDriver<NotifyUserTask>
{
    public NotifyUserTaskDisplayDriver(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
}
