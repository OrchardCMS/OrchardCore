using System;
using OpenIddict.Models;
using OrchardCore.OpenId.Abstractions.Models;

namespace OrchardCore.OpenId.EntityFrameworkCore.Models
{
    public class OpenIdAuthorization<TKey> : OpenIdAuthorization<TKey, OpenIdApplication<TKey>, OpenIdToken<TKey>>
        where TKey : IEquatable<TKey>
    {
    }

    public class OpenIdAuthorization<TKey, TApplication, TToken> : OpenIddictAuthorization<TKey, TApplication, TToken>, IOpenIdAuthorization
        where TKey : IEquatable<TKey>
    {
    }
}
