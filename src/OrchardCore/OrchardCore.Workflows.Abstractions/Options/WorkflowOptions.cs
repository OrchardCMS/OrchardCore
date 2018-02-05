using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Workflows.Options
{
    public class WorkflowOptions
    {
        public WorkflowOptions()
        {
            ActivityDictionary = new Dictionary<Type, ActivityRegistration>();
        }

        /// <summary>
        /// The set of activities available to workflows.
        /// Modules can register and unregister activities.
        /// </summary>
        private IDictionary<Type, ActivityRegistration> ActivityDictionary { get; }

        public IEnumerable<Type> ActivityTypes => ActivityDictionary.Values.Select(x => x.ActivityType).ToList().AsReadOnly();
        public IEnumerable<Type> ActivityDisplayDriverTypes => ActivityDictionary.Values.SelectMany(x => x.DriverTypes).ToList().AsReadOnly();

        public WorkflowOptions RegisterActivity(Type activityType, Type driverType = null)
        {
            if (ActivityDictionary.ContainsKey(activityType))
            {
                if (driverType != null)
                {
                    ActivityDictionary[activityType].DriverTypes.Add(driverType);
                }
            }
            else
            {
                ActivityDictionary.Add(activityType, new ActivityRegistration(activityType, driverType));
            }

            return this;
        }

        public WorkflowOptions UnregisterActivityType(Type activityType)
        {
            if (!ActivityDictionary.ContainsKey(activityType))
                throw new InvalidOperationException("The specified activity type is not registered.");

            ActivityDictionary.Remove(activityType);
            return this;
        }

        public bool IsActivityRegistered(Type activityType)
        {
            return ActivityDictionary.ContainsKey(activityType);
        }
    }

    public static class WorkflowOptionsExtensions
    {
        public static WorkflowOptions RegisterActivityType<T>(this WorkflowOptions options)
        {
            return options.RegisterActivity(typeof(T));
        }

        public static WorkflowOptions RegisterActivity<T, TDriver>(this WorkflowOptions options)
        {
            return options.RegisterActivity(typeof(T), typeof(TDriver));
        }

        public static WorkflowOptions UnregisterActivityType<T>(this WorkflowOptions options)
        {
            return options.UnregisterActivityType(typeof(T));
        }

        public static bool IsActivityRegistered<T>(this WorkflowOptions options)
        {
            return options.IsActivityRegistered(typeof(T));
        }
    }
}
