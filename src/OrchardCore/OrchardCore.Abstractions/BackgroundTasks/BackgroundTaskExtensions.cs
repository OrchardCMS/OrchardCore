using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OrchardCore.BackgroundTasks
{
    public static class BackgroundTaskExtensions
    {
        public static BackgroundTaskSettings GetDefaultSettings(this IBackgroundTask task)
        {
            var type = task.GetType();

            var attribute = type.GetCustomAttribute<BackgroundTaskAttribute>();

            if (attribute != null)
            {
                return new BackgroundTaskSettings
                {
                    Name = type.FullName,
                    Enable = attribute.Enable,
                    Schedule = attribute.Schedule,
                    Description = attribute.Description
                };
            }

            return new BackgroundTaskSettings() { Name = type.FullName };
        }

        public static IBackgroundTask GetTaskByName(this IEnumerable<IBackgroundTask> tasks, string name)
        {
            return tasks.LastOrDefault(t => t.GetTaskName() == name);
        }

        public static string GetTaskName(this IBackgroundTask task)
        {
            return task.GetType().FullName;
        }
    }
}
