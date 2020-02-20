using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IActivityLibrary
    {
        /// <summary>
        /// Returns a list of instances of all available <see cref="IActivity"/> implementations.
        /// </summary>
        IEnumerable<IActivity> ListActivities();

        /// <summary>
        /// Returns a list of all available activity categories.
        /// </summary>
        /// <returns></returns>
        IEnumerable<LocalizedString> ListCategories();

        /// <summary>
        /// Returns an activity instance with the specified name from the library.
        /// </summary>
        IActivity GetActivityByName(string name);

        /// <summary>
        /// Returns a new instance of the activity with the specified name.
        /// </summary>
        IActivity InstantiateActivity(string name);

        /// <summary>
        /// Returns new instances the specified activities.
        /// </summary>
        IEnumerable<IActivity> InstantiateActivities(IEnumerable<string> activityNames);
    }

    public static class ActivityLibraryExtensions
    {
        public static T InstantiateActivity<T>(this IActivityLibrary library, string name) where T : IActivity
        {
            return (T)library.InstantiateActivity(name);
        }

        public static T InstantiateActivity<T>(this IActivityLibrary library, string name, JObject properties) where T : IActivity
        {
            var activity = InstantiateActivity<T>(library, name);

            if (activity != null)
            {
                activity.Properties = properties;
            }

            return activity;
        }

        public static T InstantiateActivity<T>(this IActivityLibrary library, ActivityRecord record) where T : IActivity
        {
            return InstantiateActivity<T>(library, record.Name, record.Properties);
        }
    }
}
