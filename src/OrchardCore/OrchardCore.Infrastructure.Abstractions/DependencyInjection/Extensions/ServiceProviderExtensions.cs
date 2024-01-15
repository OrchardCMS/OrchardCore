using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OrchardCore.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceProviderExtensions
{
    public static IReadOnlyDictionary<object, TKeyedService> GetKeyedServiceDictionary<TKeyedService>(this IServiceProvider serviceProvider)
    {
        var keys = serviceProvider.GetRequiredService<Keys<TKeyedService>>();

        var keyedDictionary = new Dictionary<object, TKeyedService>(keys.Count);
        var keyedDictionaryWrapper = new ReadOnlyDictionary<object, TKeyedService>(keyedDictionary);

        foreach (var key in keys)
        {
            var service = serviceProvider.GetKeyedService<TKeyedService>(key);

            keyedDictionary.Add(key, service);
        }

        return keyedDictionaryWrapper;
    }
}
