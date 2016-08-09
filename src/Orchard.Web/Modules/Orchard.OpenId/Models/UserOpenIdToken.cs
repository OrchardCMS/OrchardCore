using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.OpenId.Models
{
    /// <summary>
    /// Represents an OpenId token.
    /// </summary>
    public class UserOpenIdToken
    {        
        /// <summary>
        /// Gets or sets the unique identifier
        /// associated with the current token.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the type of the current token.
        /// </summary>
        public OpenIdToken Token { get; set; }
        
    }
}
