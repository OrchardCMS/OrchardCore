using System.Collections.Immutable;
using Newtonsoft.Json.Linq;

namespace OrchardCore.OpenId.YesSql.Models
{
    public class OpenIdApplication
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
        /// Gets or sets the consent type
        /// associated with the current application.
        /// </summary>
        public string ConsentType { get; set; }

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
        /// Gets or sets the permissions associated with the application.
        /// </summary>
        public ImmutableArray<string> Permissions { get; set; }
            = ImmutableArray.Create<string>();

        /// <summary>
        /// Gets the logout callback URLs associated with the current application.
        /// </summary>
        public ImmutableArray<string> PostLogoutRedirectUris { get; set; }
            = ImmutableArray.Create<string>();

        /// <summary>
        /// Gets or sets the additional properties
        /// associated with the current application.
        /// </summary>
        public virtual JObject Properties { get; set; }

        /// <summary>
        /// Gets or sets the callback URLs associated with the current application.
        /// </summary>
        public ImmutableArray<string> RedirectUris { get; set; }
            = ImmutableArray.Create<string>();

        /// <summary>
        /// Gets or sets the roles associated with the application.
        /// </summary>
        public ImmutableArray<string> Roles { get; set; }
            = ImmutableArray.Create<string>();

        /// <summary>
        /// Gets or sets the application type
        /// associated with the current application.
        /// </summary>
        public string Type { get; set; }
    }
}
