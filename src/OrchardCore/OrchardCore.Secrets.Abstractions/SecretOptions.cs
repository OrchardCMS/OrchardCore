using System;
using System.Collections.Generic;

namespace OrchardCore.Secrets
{
    public class SecretOptions
    {
        public Dictionary<string, Type> FilterRegistrations { get; } = new Dictionary<string, Type>();
    }
}
