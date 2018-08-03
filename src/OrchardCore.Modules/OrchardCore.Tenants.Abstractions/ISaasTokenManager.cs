using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Tenants.Abstractions
{
    public interface ISaasTokenManager
    {
        string Generate();
    }
}
