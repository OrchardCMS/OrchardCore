using System;
using System.Collections.Generic;

namespace OrchardCore.Workflows.Options
{
    public class ActivityRegistration
    {
        public ActivityRegistration(Type activityType)
        {
            ActivityType = activityType;
            DriverTypes = new HashSet<Type>();
        }

        public ActivityRegistration(Type activityType, Type driverType) : this(activityType)
        {
            DriverTypes.Add(driverType);
        }

        public Type ActivityType { get; }
        public HashSet<Type> DriverTypes { get; }
    }
}
