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
                var settings = new BackgroundTaskSettings
                {
                    Title = attribute.Title,
                    Name = technicalName,
                    Enable = attribute.Enable,
                    Schedule = attribute.Schedule,
                    Description = attribute.Description,
                    LockTimeout = attribute.LockTimeout,
                    LockExpiration = attribute.LockExpiration
                };

                if (String.IsNullOrWhiteSpace(settings.Title))
                {
                    settings.Title = technicalName;
                }

                return settings;
            }

            return new BackgroundTaskSettings()
            {
                Name = technicalName,
                Title = technicalName
            };
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
