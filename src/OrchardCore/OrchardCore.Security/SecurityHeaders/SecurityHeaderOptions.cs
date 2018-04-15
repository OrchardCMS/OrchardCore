using System;
using System.Collections.Generic;

namespace OrchardCore.Security.SecurityHeaders
{
    /// <summary>
    /// Options for the security headers to be added to all responses
    /// </summary>
    public class SecurityHeaderOptions
    {
        /// <summary>
        /// A collection of headers to be added to all the server's responses.
        /// </summary>
        public IDictionary<string, string> AddHeaders { get; } = new Dictionary<string, string>();
    }
}
