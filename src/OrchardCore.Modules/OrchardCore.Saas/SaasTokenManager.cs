using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Saas
{
    class SaasTokenManager : ISaasTokenManager
    {
        public string Generate()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
