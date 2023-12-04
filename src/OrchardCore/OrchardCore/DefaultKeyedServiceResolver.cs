using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore;

public class DefaultKeyedServiceResolver(IServiceCollection serviceCollection, IServiceProvider serviceProvider) : IKeyedServiceResolver
{
    private readonly IServiceCollection _serviceCollection = serviceCollection;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IEnumerable<KeyValuePair<TKey, TService>> GetServices<TKey, TService>()
    {
        var keyType = typeof(TKey);

        var keys = _serviceCollection.Where(service => service.IsKeyedService && service.ServiceKey.GetType() == keyType && service.ServiceType == typeof(TService))
         .Select(service => (TKey)Convert.ChangeType(service.ServiceKey, keyType))
         .Distinct()
         .ToList();

        var services = new List<KeyValuePair<TKey, TService>>();

        foreach (var key in keys)
        {
            var keyedServices = _serviceProvider.GetKeyedServices<TService>(key);

            services.AddRange(keyedServices.Select(keyedService => new KeyValuePair<TKey, TService>(key, keyedService)));
        }

        return services;
    }
}
