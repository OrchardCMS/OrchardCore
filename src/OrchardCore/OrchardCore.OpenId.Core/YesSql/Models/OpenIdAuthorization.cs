using System.Collections.Immutable;
using Newtonsoft.Json.Linq;

namespace OrchardCore.OpenId.YesSql.Models
{
    public class OpenIdAuthorization
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// associated with the current application.
        /// </summary>
        public string AuthorizationId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the client application
        /// associated with the current authorization.
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the physical identifier
        /// associated with the current authorization.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the additional properties
        /// associated with the current authorization.
        /// </summary>
        public virtual JObject Properties { get; set; }

        /// <summary>
        /// Gets or sets the scopes associated with the current authorization.
        /// </summary>
        public ImmutableArray<string> Scopes { get; set; }
            = ImmutableArray.Create<string>();

        /// <summary>
        /// Gets or sets the status of the current authorization.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the subject associated with the current authorization.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the type of the current authorization.
        /// </summary>
        public string Type { get; set; }
    }
}
