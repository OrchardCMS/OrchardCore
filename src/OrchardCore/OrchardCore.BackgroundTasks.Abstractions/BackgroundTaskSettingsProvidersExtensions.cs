using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.BackgroundTasks
{

    public static class BackgroundTaskSettingsProvidersExtensions
    {
        public static async Task<BackgroundTaskSettings> GetSettingsAsync(this IEnumerable<IBackgroundTaskSettingsProvider> providers, Type type)
        {
            foreach (var provider in providers.OrderBy(p => p.Order))
            {
                var settings = await provider.GetSettingsAsync(type);

                if (settings != null && settings != BackgroundTaskSettings.None)
                {
                    return settings;
                }
            }

            return new BackgroundTaskSettings() { Name = type.FullName };
        }

        public static async Task<IEnumerable<BackgroundTaskSettings>> GetSettingsAsync(this IEnumerable<IBackgroundTaskSettingsProvider> providers, IEnumerable<Type> types)
        {
            var settings = new List<BackgroundTaskSettings>();

            foreach (var type in types)
            {
                settings.Add(await providers.GetSettingsAsync(type));
            }

            return settings;
        }

        public static IChangeToken GetChangeToken(this IEnumerable<IBackgroundTaskSettingsProvider> providers)
        {
            var changeTokens = new List<IChangeToken>();
            foreach (var provider in providers.OrderBy(p => p.Order))
            {
                var changeToken = provider.ChangeToken;
                if (changeToken != null)
                {
                    changeTokens.Add(changeToken);
                }
            }

            if (changeTokens.Count == 0)
            {
                return NullChangeToken.Singleton;
            }

            return new CompositeChangeToken(changeTokens);
        }
    }
}