using System;
using OpenIddict.Models;
using OrchardCore.OpenId.Abstractions.Models;

namespace OrchardCore.OpenId.EntityFrameworkCore.Models
{
    public class OpenIdScope<TKey> : OpenIddictScope<TKey>, IOpenIdScope
        where TKey : IEquatable<TKey>
    {
        // Warning: to keep this entity compatible with standalone OpenIddict deployments,
        // no property SHOULD be added to this model class. Instead, consider using the
        // IOpenIdScopeStore.GetPropertiesAsync/SetPropertiesAsync() methods,
        // that allow storing additional properties without requiring schema changes.
    }
}
