using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.OpenIdConnect
{
    public class OpenIdConnectSettings
    {
        public string DisplayName { get; set; }
        //
        // Summary:
        //     Gets or sets if HTTPS is required for the metadata address or authority. The
        //     default is false. This should be enabled only in development environments.
        public bool TestingModeEnabled { get; set; }

        //
        // Summary:
        //     Gets or sets the Authority to use when making OpenIdConnect calls.
        public string Authority { get; set; }
        //
        // Summary:
        //     Gets or sets the 'client_id'.
        public string ClientId { get; set; }
        //
        // Summary:
        //     Gets or sets the 'client_secret'.
        public string ClientSecret { get; set; }
        //
        // Summary:
        //     The request path within the application's base path where the user-agent will
        //     be returned. The middleware will process this request when it arrives.
        public PathString CallbackPath { get; set; }
        //
        // Summary:
        //     The uri where the user agent will be redirected to after application is signed
        //     out from the identity provider. The redirect will happen after the SignedOutCallbackPath
        //     is invoked.
        //
        // Remarks:
        //     This URI can be out of the application's domain. By default it points to the
        //     root.
        public string SignedOutRedirectUri { get; set; }

        //
        // Summary:
        //     The request path within the application's base path where the user agent will
        //     be returned after sign out from the identity provider. See post_logout_redirect_uri
        //     from http://openid.net/specs/openid-connect-session-1_0.html#RedirectionAfterLogout.
        public PathString SignedOutCallbackPath { get; set; }

        //
        // Summary:
        //     Gets the list of permissions to request, space seperated.
        public IEnumerable<string> AllowedScopes { get; set; }
    }
}
