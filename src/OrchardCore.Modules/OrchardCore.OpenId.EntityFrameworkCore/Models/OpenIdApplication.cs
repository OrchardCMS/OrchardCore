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
        /// <summary>
        /// Gets or sets if a consent form has to be fulfilled by 
        /// the user after log in.
        /// </summary>
        public bool SkipConsent { get; set; }

        public bool AllowPasswordFlow { get; set; }
        public bool AllowClientCredentialsFlow { get; set; }
        public bool AllowAuthorizationCodeFlow { get; set; }
        public bool AllowRefreshTokenFlow { get; set; }
        public bool AllowImplicitFlow { get; set; }
        public bool AllowHybridFlow { get; set; }
    }
}
