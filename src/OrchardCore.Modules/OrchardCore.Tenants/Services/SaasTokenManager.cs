using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.Tenants.Abstractions;

namespace OrchardCore.Tenants.Services
{
    class SaasTokenManager : ISaasTokenManager
    {
        public string Generate()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
