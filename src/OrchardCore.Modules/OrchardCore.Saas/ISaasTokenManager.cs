using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Saas
{
    public interface ISaasTokenManager
    {
        string Generate();
    }
}
