using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.GitHub.Configuration
{
    /// <summary>
    /// Configuration options for <see cref="GitHubHandler"/>.
    /// </summary>
    public class GitHubOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new <see cref="GitHubOptions"/>.
        /// </summary>
        public GitHubOptions()
        {
            CallbackPath = new PathString("/signin-github");
            AuthorizationEndpoint = GitHubDefaults.AuthorizationEndpoint;
            TokenEndpoint = GitHubDefaults.TokenEndpoint;
            UserInformationEndpoint = GitHubDefaults.UserInformationEndpoint;

            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            ClaimActions.MapJsonKey("name", "login");
            ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.Email);
            ClaimActions.MapJsonKey("url", "url");
        }
    }
}
