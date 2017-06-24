using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.OpenId.Models
{
    public class OpenIdAuthorization
    {
        public virtual int Id { get; set; }

        /// <summary>
        /// Gets or sets the space-delimited scopes
        /// associated with the current authorization.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets the list of tokens
        /// associated with the current authorization.
        /// </summary>
        public IList<OpenIdToken> Tokens { get; } = new List<OpenIdToken>();
    }
}
