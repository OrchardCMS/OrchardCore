using System;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Email.Services;

namespace OrchardCore.Email.Core.Services;

public class EmailDeliveryServiceResolver : IEmailDeliveryServiceResolver
{
    private readonly IServiceProvider _serviceProvider;

    public EmailDeliveryServiceResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEmailDeliveryService Resolve(string name) =>
        _serviceProvider.GetKeyedService<IEmailDeliveryService>(name) ?? _serviceProvider.GetRequiredService<IEmailDeliveryService>();
}
