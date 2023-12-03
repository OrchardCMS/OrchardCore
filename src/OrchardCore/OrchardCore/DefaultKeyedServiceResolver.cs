using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore;

public class DefaultKeyedServiceResolver(IServiceCollection services) : IKeyedServiceResolver
{
    private readonly IServiceCollection _services = services;

    public IReadOnlyDictionary<TKey, TService> GetServices<TKey, TService>()
        => _services.Where(service => service.ServiceKey is not null && service.ServiceKey!.GetType() == typeof(TKey) && service.ServiceType == typeof(TService))
                 .ToFrozenDictionary(service => (TKey)Convert.ChangeType(service.ServiceKey, typeof(TKey)), service => (TService)service.KeyedImplementationInstance);
}
