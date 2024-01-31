using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Options;

namespace OrchardCore.Workflows.Services
{
    public class ActivityLibrary : IActivityLibrary
    {
        private readonly Lazy<IDictionary<string, IActivity>> _activityDictionary;
        private readonly Lazy<IList<LocalizedString>> _activityCategories;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public ActivityLibrary(IOptions<WorkflowOptions> workflowOptions, IServiceProvider serviceProvider, ILogger<ActivityLibrary> logger)
        {
            _activityDictionary = new Lazy<IDictionary<string, IActivity>>(() => workflowOptions.Value.ActivityTypes.Where(x => !x.IsAbstract).Select(x => serviceProvider.CreateInstance<IActivity>(x)).OrderBy(x => x.Name).ToDictionary(x => x.Name));
            _activityCategories = new Lazy<IList<LocalizedString>>(() => _activityDictionary.Value.Values.OrderBy(x => x.Category.Value).Select(x => x.Category).Distinct(new LocalizedStringComparer()).ToList());
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        private IDictionary<string, IActivity> ActivityDictionary => _activityDictionary.Value;
        private IList<LocalizedString> ActivityCategories => _activityCategories.Value;

        public IEnumerable<IActivity> ListActivities()
        {
            return ActivityDictionary.Values;
        }

        public IEnumerable<LocalizedString> ListCategories()
        {
            return ActivityCategories;
        }

        public IActivity GetActivityByName(string name)
        {
            return ActivityDictionary.TryGetValue(name, out var activity) ? activity : null;
        }

        public IActivity InstantiateActivity(string name)
        {
            var activityType = GetActivityByName(name)?.GetType();

            if (activityType == null)
            {
                _logger.LogWarning("Requested activity '{ActivityName}' does not exist in the library. This could indicate a changed name or a missing feature.", name);
                return null;
            }

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
