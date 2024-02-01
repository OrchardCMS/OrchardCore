using System;
using Newtonsoft.Json.Linq;

namespace OrchardCore.OpenId.YesSql.Models
{
    /// <summary>
    /// Represents an OpenId token.
    /// </summary>
    public class OpenIdToken
    {
        /// <summary>
        /// The name of the collection that is used for this type.
        /// </summary>
        public const string OpenIdCollection = "OpenId";

        /// <summary>
        /// Gets or sets the unique identifier associated with the current token.
        /// </summary>
        public string TokenId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the application associated with the current token.
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the authorization associated with the current token.
        /// </summary>
        public string AuthorizationId { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the current token.
        /// </summary>
        public DateTime? CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of the current token.
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the physical identifier associated with the current token.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the payload of the current token, if applicable.
        /// Note: this property is only used for reference tokens
        /// and may be encrypted for security reasons.
        /// </summary>
        public string Payload { get; set; }

        /// <summary>
        /// Gets or sets the additional properties associated with the current token.
        /// </summary>
        public JObject Properties { get; set; }

        /// <summary>
        /// Gets or sets the redemption date of the current token.
        /// </summary>
        public DateTime? RedemptionDate { get; set; }

        /// <summary>
        /// Gets or sets the reference identifier associated
        /// with the current token, if applicable.
        /// Note: this property is only used for reference tokens
        /// and may be hashed or encrypted for security reasons.
        /// </summary>
        public string ReferenceId { get; set; }

        /// <summary>
        /// Gets or sets the status of the current token.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the subject associated with the current token.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the type of the current token.
        /// </summary>
        public string Type { get; set; }
    }
}
