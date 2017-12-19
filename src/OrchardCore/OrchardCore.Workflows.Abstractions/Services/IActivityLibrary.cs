using System.Collections.Generic;

namespace OrchardCore.Workflows.Services
{
    public interface IActivityLibrary
    {
        /// <summary>
        /// Returns a list of instances of all available <see cref="IActivity"/> implementations.
        /// </summary>
        IEnumerable<IActivity> ListActivities();

        /// <summary>
        /// Returns an activity instance with the specified name from the library.
        /// </summary>
        IActivity GetActivityByName(string name);

        /// <summary>
        /// Returns a new instance of the activity with the specified name.
        /// </summary>
        IActivity CreateActivity(string name);

        /// <summary>
        /// Returns a new copy of all available <see cref="IActivity"/> implementations.
        /// </summary>
        IEnumerable<IActivity> CreateActivities();
    }
}