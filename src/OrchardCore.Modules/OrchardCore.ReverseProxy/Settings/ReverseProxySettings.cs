using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;

namespace OrchardCore.ReverseProxy.Settings
{
    public class ReverseProxySettings
    {
        public ForwardedHeaders ForwardedHeaders { get; set; }
    }
}
