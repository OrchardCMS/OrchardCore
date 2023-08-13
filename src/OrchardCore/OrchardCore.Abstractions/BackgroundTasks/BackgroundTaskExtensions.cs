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
                    Name = technicalName,
                    Title = !String.IsNullOrWhiteSpace(attribute.Title) ? attribute.Title : technicalName,
                    Enable = attribute.Enable,
                    Schedule = attribute.Schedule,
                    Description = attribute.Description,
                    LockTimeout = attribute.LockTimeout,
                    LockExpiration = attribute.LockExpiration,
                    UsePipeline = attribute.UsePipeline,
                };
            }

            return new BackgroundTaskSettings()
            {
                Name = technicalName,
                Title = technicalName,
            };
        }

        public static IBackgroundTask GetTaskByName(this IEnumerable<IBackgroundTask> tasks, string name)
            => tasks.LastOrDefault(task => task.GetTaskName() == name);

        public static string GetTaskName(this IBackgroundTask task) => task.GetType().FullName;
    }
}
