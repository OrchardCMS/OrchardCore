using Microsoft.Extensions.Logging;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Abstractions.Stores;

namespace OrchardCore.OpenId.Services.Managers
{
    public class OpenIdAuthorizationManager : OpenIddictAuthorizationManager<IOpenIdAuthorization>
    {
        public OpenIdAuthorizationManager(
            IOpenIdAuthorizationStore store,
            ILogger<OpenIdAuthorizationManager> logger)
            : base(store, logger)
        {
        }
    }
}
