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
        // Warning: to keep this entity compatible with standalone OpenIddict deployments,
        // no property SHOULD be added to this model class. Instead, consider using the
        // IOpenIdTokenStore.GetPropertiesAsync/SetPropertiesAsync() methods,
        // that allow storing additional properties without requiring schema changes.
    }
}
