using Microsoft.Extensions.Logging;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Abstractions.Stores;

namespace OrchardCore.OpenId.Services.Managers
{
    public class OpenIdScopeManager : OpenIddictScopeManager<IOpenIdScope>
    {
        public OpenIdScopeManager(
            IOpenIdScopeStore store,
            ILogger<OpenIdScopeManager> logger)
            : base(store, logger)
        {
        }
    }
}
