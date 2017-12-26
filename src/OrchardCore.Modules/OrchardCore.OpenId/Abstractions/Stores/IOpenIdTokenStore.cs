using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Models;

namespace OrchardCore.OpenId.Abstractions.Stores
{
    public interface IOpenIdTokenStore : IOpenIddictTokenStore<IOpenIdToken>
    {
    }
}