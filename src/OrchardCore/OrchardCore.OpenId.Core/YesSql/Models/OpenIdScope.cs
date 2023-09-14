using System.Collections.Immutable;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace OrchardCore.OpenId.YesSql.Models
{
    public class OpenIdScope
    {
        /// <summary>
        /// The name of the collection that is used for this type.
        /// </summary>
        public const string OpenIdCollection = "OpenId";

        /// <summary>
        /// Gets or sets the unique identifier associated with the current scope.
        /// </summary>
        public string ScopeId { get; set; }

        /// <summary>
        /// Gets or sets the public description associated with the current scope.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the localized descriptions associated with the scope.
        /// </summary>
        public ImmutableDictionary<CultureInfo, string> Descriptions { get; set; }
            = ImmutableDictionary.Create<CultureInfo, string>();

        /// <summary>
        /// Gets or sets the display name associated with the current scope.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the localized display names associated with the scope.
        /// </summary>
        public ImmutableDictionary<CultureInfo, string> DisplayNames { get; set; }
            = ImmutableDictionary.Create<CultureInfo, string>();

        /// <summary>
        /// Gets or sets the physical identifier associated with the current scope.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the unique name associated with the current scope.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the additional properties associated with the current scope.
        /// </summary>
        public JObject Properties { get; set; }

        /// <summary>
        /// Gets or sets the resources associated with the current scope.
        /// </summary>
        public ImmutableArray<string> Resources { get; set; }
            = ImmutableArray.Create<string>();
    }
}
