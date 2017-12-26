using System;
using OpenIddict.Models;
using OrchardCore.OpenId.Abstractions.Models;

namespace OrchardCore.OpenId.EntityFrameworkCore.Models
{
    /// <summary>
    /// Represents an OpenId token.
    /// </summary>
    public class OpenIdToken<TKey> : OpenIddictToken<TKey, OpenIdApplication<TKey>, OpenIdAuthorization<TKey>>, IOpenIdToken
        where TKey : IEquatable<TKey>
    {
    }
}
