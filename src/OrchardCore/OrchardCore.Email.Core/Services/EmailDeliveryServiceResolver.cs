using System;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Email.Services;

namespace OrchardCore.Email.Core.Services;

public class EmailDeliveryServiceResolver : IEmailDeliveryServiceResolver
{
    private static IEmailDeliveryService _nullEmailDeliveryService;

    private readonly IServiceProvider _serviceProvider;

    public EmailDeliveryServiceResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEmailDeliveryService Resolve(string name)
    {
        _nullEmailDeliveryService ??= _serviceProvider.GetKeyedService<IEmailDeliveryService>(EmailConstants.NullEmailDeliveryServiceName);

        return _serviceProvider.GetKeyedService<IEmailDeliveryService>(name) ?? _nullEmailDeliveryService;
    }
}
