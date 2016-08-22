using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.OpenId.Models
{
    public class OpenIdScope
    {
        /// <summary>
        /// Gets or sets the public description
        /// associated with the current scope.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier
        /// associated with the current scope.
        /// </summary>
        public int Id { get; set; }
    }
}
