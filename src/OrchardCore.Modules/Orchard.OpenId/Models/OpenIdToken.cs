using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.OpenId.Models
{
    /// <summary>
    /// Represents an OpenId token.
    /// </summary>
    public class OpenIdToken
    {        
        /// <summary>
        /// Gets or sets the unique identifier
        /// associated with the current token.
        /// </summary>
        public int Id { get; set; }

        /// Gets or sets the UserId
        /// associated with the current token.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the type of the current token.
        /// </summary>
        public string Type { get; set; }

        /// Gets or sets the OpenIdApplicationId
        /// associated with the current token.
        /// </summary>
        public int AppId { get; set; }

    }
}
