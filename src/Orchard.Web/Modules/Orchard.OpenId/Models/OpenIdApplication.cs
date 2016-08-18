using Newtonsoft.Json;
using OpenIddict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.OpenId.Models
{
    public class OpenIdApplication
    {
        /// <summary>
        /// Gets or sets the client identifier
        /// associated with the current application.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the hashed client secret
        /// associated with the current application.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the display name
        /// associated with the current application.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier
        /// associated with the current application.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the logout callback URL
        /// associated with the current application.
        /// </summary>
        public string LogoutRedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the callback URL
        /// associated with the current application.
        /// </summary>
        public string RedirectUri { get; set; }

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
        public List<string> RoleNames { get; set; } = new List<string>();
    }

    public enum ClientType
    {
        Confidential,
        Public
    }
}
