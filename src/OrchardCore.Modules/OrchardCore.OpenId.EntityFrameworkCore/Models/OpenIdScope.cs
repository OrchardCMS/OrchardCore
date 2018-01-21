using System;
using OpenIddict.Models;
using OrchardCore.OpenId.Abstractions.Models;

namespace OrchardCore.OpenId.EntityFrameworkCore.Models
{
    public class OpenIdScope<TKey> : OpenIddictScope<TKey>, IOpenIdScope
        where TKey : IEquatable<TKey>
    {
    }
}
