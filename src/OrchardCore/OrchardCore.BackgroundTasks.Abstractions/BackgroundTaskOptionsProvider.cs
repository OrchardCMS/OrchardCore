using System;
using System.Reflection;
using System.Threading.Tasks;

namespace OrchardCore.BackgroundTasks
{
    public interface IBackgroundTaskOptionsProvider
    {
        int Order { get; }
        Task<BackgroundTaskOptions> GetOptionsAsync(Type type);
    }

    public class BackgroundTaskOptionsProvider : IBackgroundTaskOptionsProvider
    {
        public int Order => 100;

        public Task<BackgroundTaskOptions> GetOptionsAsync(Type type)
        {
            var attribute = type.GetCustomAttribute<BackgroundTaskAttribute>();

            if (attribute != null)
            {
                return Task.FromResult(new BackgroundTaskOptions
                {
                    Enable = attribute.Enable,
                    Schedule = attribute.Schedule
                });
            }

            return Task.FromResult(new NotFoundBackgroundTaskOptions() as BackgroundTaskOptions);
        }
    }
}