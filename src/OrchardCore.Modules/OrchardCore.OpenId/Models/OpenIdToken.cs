using System;

namespace OrchardCore.OpenId.Models
{
    /// <summary>
    /// Represents an OpenId token.
    /// </summary>
    public class OpenIdToken
    {
        /// Gets or sets the identifier of the application
        /// associated with the current token.
        /// </summary>
        public int? ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the authorization
        /// associated with the current token.
        /// </summary>
        public int? AuthorizationId { get; set; }

        /// <summary>
        /// Gets or sets the date on which the token
        /// will start to be considered valid.
        /// </summary>
        public DateTimeOffset? CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the date on which the token
        /// will no longer be considered valid.
        /// </summary>
        public DateTimeOffset? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier
        /// associated with the current token.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the payload of the current token, if applicable.
        /// Note: this property is only used for reference tokens
        /// and may be encrypted for security reasons.
        /// </summary>
        public string Payload { get; set; }

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
