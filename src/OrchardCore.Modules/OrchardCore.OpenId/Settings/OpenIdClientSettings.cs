using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.OpenId.Settings
{
    public class OpenIdClientSettings
    {
        public string DisplayName { get; set; }
        /// <summary>
        ///     Gets or sets if HTTPS is required for the metadata address or authority. The
        ///     default is false. This should be enabled only in development environments.
        /// </summary>
        public bool TestingModeEnabled { get; set; }
        /// <summary>
        ///     Gets or sets the Authority to use when making OpenIdConnect calls.
        /// </summary>
        public string Authority { get; set; }
        /// <summary>
        ///     Gets or sets the 'client_id'.
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        ///     Gets or sets the 'client_secret'.
        /// </summary>
        public string ClientSecret { get; set; }
        /// <summary>
        ///     The request path within the application's base path where the user-agent will
        ///     be returned. The middleware will process this request when it arrives.
        /// </summary>
        public PathString CallbackPath { get; set; }
        /// <summary>
        ///     The uri where the user agent will be redirected to after application is signed
        ///     out from the identity provider. The redirect will happen after the SignedOutCallbackPath
        ///     is invoked.
        /// </summary>
        /// <remarks>
        ///     This URI can be out of the application's domain. By default it points to the
        ///     root.
        /// </remarks>
        public string SignedOutRedirectUri { get; set; }
        /// <summary>
        ///     The request path within the application's base path where the user agent will
        ///     be returned after sign out from the identity provider. See post_logout_redirect_uri
        ///     from http://openid.net/specs/openid-connect-session-1_0.html#RedirectionAfterLogout.
        /// </summary>
        public PathString SignedOutCallbackPath { get; set; }

        /// <summary>
        ///     Gets the list of permissions to request, space seperated.
        /// </summary>
        public IEnumerable<string> AllowedScopes { get; set; }
    }
}
