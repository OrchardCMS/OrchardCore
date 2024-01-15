using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.DependencyInjection;

public class OrchardCoreServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
{
    public IServiceCollection CreateBuilder(IServiceCollection services) => services;

    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
    {
        var keyedDictionary = new Dictionary<Type, List<object>>();

        foreach (var service in containerBuilder)
        {
            if (service.ServiceKey != null)
            {
                if (!keyedDictionary.TryGetValue(service.ServiceType, out var keysList))
                {
                    keysList = [];
                    keyedDictionary[service.ServiceType] = keysList;
                }

                keysList.Add(service.ServiceKey);
            }
        }

        AddKeysService(containerBuilder, keyedDictionary);

        containerBuilder.AddSingleton(typeof(Keys<>));

        return containerBuilder.BuildServiceProvider();
    }

    private static void AddKeysService(IServiceCollection services, IDictionary<Type, List<object>> keyedDictionary)
    {
        foreach (var keyedDictionaryItem in keyedDictionary)
        {
            var serviceType = typeof(Keys<>).MakeGenericType(keyedDictionaryItem.Key);
            var service = Activator.CreateInstance(serviceType, keyedDictionaryItem.Value);

            services.AddSingleton(serviceType, service!);
        }
    }
}
