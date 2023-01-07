using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OrchardCore.BackgroundTasks
{
    public static class BackgroundTaskExtensions
    {
        public static BackgroundTaskSettings GetDefaultSettings(this IBackgroundTask task)
        {
            var technicalName = task.GetTaskName();

            var attribute = task.GetType().GetCustomAttribute<BackgroundTaskAttribute>();

            if (attribute != null)
            {
                return new BackgroundTaskSettings
                {
                    Title = !String.IsNullOrWhiteSpace(attribute.Title) ? attribute.Title : technicalName,
                    Name = technicalName,
                    Enable = attribute.Enable,
                    Schedule = attribute.Schedule,
                    Description = attribute.Description,
                    LockTimeout = attribute.LockTimeout,
                    LockExpiration = attribute.LockExpiration
                };
            }

            return new BackgroundTaskSettings()
            {
                Name = technicalName,
                Title = technicalName
            };
        }

        public static IBackgroundTask GetTaskByName(this IEnumerable<IBackgroundTask> tasks, string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return null;
            }

            return tasks.LastOrDefault(task => String.Equals(task.GetTaskName(), name, StringComparison.OrdinalIgnoreCase));
        }

        public static string GetTaskName(this IBackgroundTask task)
        {
            return task.GetType().FullName;
        }
    }
}
