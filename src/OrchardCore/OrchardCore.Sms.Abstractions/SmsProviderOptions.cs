using System;
using System.Collections.Generic;

namespace OrchardCore.Sms;

public class SmsProviderOptions
{
    /// <summary>
    /// This collections is used to register new SMS provider.
    /// The 'Key' is the name of the service.
    /// The 'Value' is a callback function that return a provider.
    /// </summary>
    public Dictionary<string, Func<IServiceProvider, ISmsProvider>> Providers { get; } = new();
}
