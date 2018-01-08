using System;
using OpenIddict.Models;
using OrchardCore.OpenId.Abstractions.Models;

namespace OrchardCore.OpenId.EntityFrameworkCore.Models
{
    public class OpenIdToken<TKey> : OpenIdToken<TKey, OpenIdApplication<TKey>, OpenIdAuthorization<TKey>>
        where TKey : IEquatable<TKey>
    {
    }

    public class OpenIdToken<TKey, TApplication, TAuthorization> : OpenIddictToken<TKey, TApplication, TAuthorization>, IOpenIdToken
        where TKey : IEquatable<TKey>
    {
    }
}
