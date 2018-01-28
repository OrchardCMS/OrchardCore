using System;
using System.Collections.Generic;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Models;

namespace OrchardCore.OpenId.YesSql.Models
{
    public class OpenIdApplication : IOpenIdApplication
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// associated with the current application.
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the client identifier
        /// associated with the current application.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret associated with the current application.
        /// Note: depending on the application manager used to create this instance,
        /// this property may be hashed or encrypted for security reasons.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the display name
        /// associated with the current application.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the physical identifier
        /// associated with the current application.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the logout callback URLs
        /// associated with the current application.
        /// </summary>
        public ISet<string> PostLogoutRedirectUris { get; set; }
            = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>
        /// Gets or sets the callback URLs
        /// associated with the current application.
        /// </summary>
        public ISet<string> RedirectUris { get; set; }
            = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>
        /// Gets or sets the application type
        /// associated with the current application.        
        /// </summary>
        public ClientType Type { get; set; }

        /// <summary>
        /// Gets or sets if a consent form has to be fulfilled by 
        /// the user after log in.
        /// </summary>
        public bool SkipConsent { get; set; }

        /// <summary>
        /// Gets or sets the RoleNames assined to the app.
        /// </summary>
        public ISet<string> RoleNames { get; set; }
            = new HashSet<string>(StringComparer.Ordinal);

        public bool AllowPasswordFlow { get; set; }
        public bool AllowClientCredentialsFlow { get; set; }
        public bool AllowAuthorizationCodeFlow { get; set; }
        public bool AllowRefreshTokenFlow { get; set; }
        public bool AllowImplicitFlow { get; set; }
        public bool AllowHybridFlow { get; set; }
    }
}
