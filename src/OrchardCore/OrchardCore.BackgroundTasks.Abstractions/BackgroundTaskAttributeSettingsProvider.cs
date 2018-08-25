using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskAttributeSettingsProvider : IBackgroundTaskSettingsProvider
    {
        public int Order => 100;

        public IChangeToken ChangeToken => NullChangeToken.Singleton;

        public Task<BackgroundTaskSettings> GetSettingsAsync(Type type)
        {
            var attribute = type.GetCustomAttribute<BackgroundTaskAttribute>();

            if (attribute != null)
            {
                return Task.FromResult(new BackgroundTaskSettings
                {
                    Name = type.FullName,
                    Enable = attribute.Enable,
                    Schedule = attribute.Schedule,
                    Description = attribute.Description
                });
            }

            return Task.FromResult(BackgroundTaskSettings.None);
        }
    }
}