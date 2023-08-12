using System;
using System.Collections.Generic;
using OrchardCore.Sms.Abstractions;

namespace OrchardCore.Sms.Services;

public class SmsServiceOptions
{
    public string Name { get; set; } = "Console";

    public Dictionary<string, Func<IServiceProvider, ISmsService>> SmsServices { get; } = new();
}
