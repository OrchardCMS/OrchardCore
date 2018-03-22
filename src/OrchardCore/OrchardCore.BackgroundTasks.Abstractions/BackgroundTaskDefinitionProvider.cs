using System;
using System.Reflection;
using System.Threading.Tasks;

namespace OrchardCore.BackgroundTasks
{
    public interface IBackgroundTaskDefinitionProvider
    {
        int Order { get; }
        Task<BackgroundTaskDefinition> GetDefinitionAsync(Type type);
    }

    public class BackgroundTaskDefinitionProvider : IBackgroundTaskDefinitionProvider
    {
        public int Order => 100;

        public Task<BackgroundTaskDefinition> GetDefinitionAsync(Type type)
        {
            var attribute = type.GetCustomAttribute<BackgroundTaskAttribute>();

            if (attribute != null)
            {
                return Task.FromResult(new BackgroundTaskDefinition
                {
                    Enable = attribute.Enable,
                    Description = attribute.Description,
                    Schedule = attribute.Schedule
                });
            }

            return Task.FromResult(new NotFoundBackgroundTaskDefinition() as BackgroundTaskDefinition);
        }
    }
}