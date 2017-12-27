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

        public ActivityLibrary(Func<IEnumerable<IActivity>> activityLibrary, IServiceProvider serviceProvider)
        {
            _activityDictionary = new Lazy<IDictionary<string, IActivity>>(() => activityLibrary().OrderBy(x => x.Name).ToDictionary(x => x.Name));
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

        public IActivity InstantiateActivity(string name)
        {
            var activityType = GetActivityByName(name).GetType();
            return InstantiateActivity(activityType);
        }

        public IEnumerable<IActivity> InstantiateActivities(IEnumerable<string> activityNames)
        {
            var activityNameList = activityNames.ToList();
            foreach (var activitySample in ActivityDictionary.Values.Where(x => activityNameList.Contains(x.Name)))
            {
                yield return InstantiateActivity(activitySample.GetType());
            }
        }

        private IActivity InstantiateActivity(Type activityType)
        {
            return _serviceProvider.CreateInstance<IActivity>(activityType);
        }
    }
}