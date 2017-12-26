using Microsoft.Extensions.Logging;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Abstractions.Stores;

namespace OrchardCore.OpenId.Services.Managers
{
    public class OpenIdTokenManager : OpenIddictTokenManager<IOpenIdToken>
    {
        public OpenIdTokenManager(
            IOpenIdTokenStore store,
            ILogger<OpenIdTokenManager> logger)
            : base(store, logger)
        {
        }
    }
}
