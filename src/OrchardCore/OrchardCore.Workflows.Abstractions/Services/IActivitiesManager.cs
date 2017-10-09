using System.Collections.Generic;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IActivitiesManager
    {
        IEnumerable<IActivity> GetActivities();
        IActivity GetActivityByName(string name);
    }
}