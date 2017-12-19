using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.Workflows.Abstractions.Helpers;

namespace OrchardCore.Workflows.Services
{
    public class ActivityLibrary : IActivityLibrary
    {
        private readonly Lazy<IDictionary<string, IActivity>> _activityDictionary;
        private readonly IServiceProvider _serviceProvider;

        public ActivityLibrary(IEnumerable<IActivity> activityLibrary, IServiceProvider serviceProvider)
        {
            _activityDictionary = new Lazy<IDictionary<string, IActivity>>(() => activityLibrary.OrderBy(x => x.Name).ToDictionary(x => x.Name));
            _serviceProvider = serviceProvider;
        }

        private IDictionary<string, IActivity> ActivityDictionary => _activityDictionary.Value;

        public IEnumerable<IActivity> ListActivities()
        {
            return ActivityDictionary.Values;
        }

        public IActivity GetActivityByName(string name)
        {
            return ActivityDictionary.ContainsKey(name) ? ActivityDictionary[name] : null;
        }

        public IActivity CreateActivity(string name)
        {
            var activityType = GetActivityByName(name).GetType();
            return CreateActivity(activityType);
        }

        public IEnumerable<IActivity> CreateActivities()
        {
            foreach (var activitySample in ActivityDictionary.Values)
            {
                yield return CreateActivity(activitySample.GetType());
            }
        }

        private IActivity CreateActivity(Type activityType)
        {
            return _serviceProvider.CreateInstance<IActivity>(activityType);
        }
    }
}