using System;
using OpenIddict.Models;
using OrchardCore.OpenId.Abstractions.Models;

namespace OrchardCore.OpenId.EntityFrameworkCore.Models
{
    public class OpenIdApplication<TKey> : OpenIdApplication<TKey, OpenIdAuthorization<TKey>, OpenIdToken<TKey>>
        where TKey : IEquatable<TKey>
    {
    }

    public class OpenIdApplication<TKey, TAuthorization, TToken> : OpenIddictApplication<TKey, TAuthorization, TToken>, IOpenIdApplication
        where TKey : IEquatable<TKey>
    {
        // Warning: to keep this entity compatible with standalone OpenIddict deployments,
        // no property SHOULD be added to this model class. Instead, consider using the
        // IOpenIdApplicationStore.GetPropertiesAsync/SetPropertiesAsync() methods,
        // that allow storing additional properties without requiring schema changes.
    }
}
