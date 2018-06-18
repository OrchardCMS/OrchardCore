using System;
using System.Collections.Generic;
using OpenIddict.Abstractions;

namespace OrchardCore.OpenId.Abstractions.Descriptors
{
    public class OpenIdApplicationDescriptor : OpenIddictApplicationDescriptor
    {
        /// <summary>
        /// Gets the roles associated with the application.
        /// </summary>
        public ISet<string> Roles { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}
