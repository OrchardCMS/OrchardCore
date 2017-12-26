using System;
using OpenIddict.Models;
using OrchardCore.OpenId.Abstractions.Models;

namespace OrchardCore.OpenId.EntityFrameworkCore.Models
{
    public class OpenIdAuthorization<TKey> : OpenIddictAuthorization<TKey, OpenIdApplication<TKey>, OpenIdToken<TKey>>, IOpenIdAuthorization
        where TKey : IEquatable<TKey>
    {
    }
}
