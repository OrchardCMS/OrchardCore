using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Github.Configuration
{
    /// <summary>
    /// Configuration options for <see cref="MicrosoftAccountHandler"/>.
    /// </summary>
    public class GithubOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new <see cref="MicrosoftAccountOptions"/>.
        /// </summary>
        public GithubOptions()
        {
            CallbackPath = new PathString("/signin-github");
            AuthorizationEndpoint = GithubDefaults.AuthorizationEndpoint;
            TokenEndpoint = GithubDefaults.TokenEndpoint;
            UserInformationEndpoint = GithubDefaults.UserInformationEndpoint;

            
            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            ClaimActions.MapJsonKey("name", "login");
            ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.Email);
            ClaimActions.MapJsonKey("url", "url");
        }
    }
}