using System.Collections.Immutable;
using Newtonsoft.Json.Linq;

namespace OrchardCore.OpenId.YesSql.Models
{
    public class OpenIdScope
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// associated with the current scope.
        /// </summary>
        public string ScopeId { get; set; }

        /// <summary>
        /// Gets or sets the public description
        /// associated with the current scope.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the display name
        /// associated with the current scope.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the physical identifier
        /// associated with the current scope.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique name
        /// associated with the current scope.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the additional properties
        /// associated with the current scope.
        /// </summary>
        public virtual JObject Properties { get; set; }

        /// <summary>
        /// Gets or sets the resources associated with the current scope.
        /// </summary>
        public virtual ImmutableArray<string> Resources { get; set; }
            = ImmutableArray.Create<string>();
    }
}
