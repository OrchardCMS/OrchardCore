using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Email.Services;

namespace OrchardCore.Email;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddEmailServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailMessageValidator, EmailMessageValidator>();
        services.AddEmailDeliveryService<NullEmailDeliveryService>(EmailConstants.NullEmailDeliveryServiceName);
        services.AddScoped<IEmailDeliveryServiceResolver, EmailDeliveryServiceResolver>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }

    public static IServiceCollection AddEmailDeliveryService<TEmailDeliveryService>(this IServiceCollection services, string key)
        where TEmailDeliveryService : class, IEmailDeliveryService
    {
        services.AddKeyedScoped<IEmailDeliveryService, TEmailDeliveryService>(key);

        var keyedDictionary = new Dictionary<Type, List<object>>();

        foreach (var service in services.Where(s => s.ServiceType.Name == nameof(IEmailDeliveryService)))
        {
            if (!keyedDictionary.TryGetValue(service.ServiceType, out var keysList))
            {
                keysList = [];
                keyedDictionary[service.ServiceType] = keysList;
            }

            keysList.Add(service.ServiceKey);
        }

        AddKeysService(services, keyedDictionary);

        services.AddSingleton(typeof(EmailDeliveryServiceKeys));

        return services;
    }

    internal static IReadOnlyDictionary<object, IEmailDeliveryService> GetEmailDeliveryServiceDictionary(this IServiceProvider serviceProvider)
    {
        var keys = serviceProvider.GetRequiredService<EmailDeliveryServiceKeys>();

        var keyedDictionary = new Dictionary<object, IEmailDeliveryService>(keys.Count);
        var keyedDictionaryWrapper = new ReadOnlyDictionary<object, IEmailDeliveryService>(keyedDictionary);

        foreach (var key in keys)
        {
            var service = serviceProvider.GetKeyedService<IEmailDeliveryService>(key);

            keyedDictionary.Add(key, service);
        }

        return keyedDictionaryWrapper;
    }

    private static void AddKeysService(IServiceCollection services, IDictionary<Type, List<object>> keyedDictionary)
    {
        foreach (var keyedDictionaryItem in keyedDictionary)
        {
            var serviceType = typeof(EmailDeliveryServiceKeys);
            var service = Activator.CreateInstance(serviceType, keyedDictionaryItem.Value);

            services.AddSingleton(serviceType, service!);
        }
    }
}
