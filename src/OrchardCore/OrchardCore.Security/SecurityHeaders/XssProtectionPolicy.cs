namespace OrchardCore.Security.SecurityHeaders
{
    /// <summary>
    /// Options for the X-XSS-Protection header
    /// </summary>
    /// <remarks>
    /// See: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-XSS-Protection
    /// </remarks>
    public enum XssProtectionPolicy
    {
        /// <summary>
        /// Disables XSS filtering.
        /// </summary>
        DisableFiltering,

        /// <summary>
        /// Enables XSS filtering (usually default in browsers). If a cross-site scripting attack is detected, 
        /// the browser will sanitize the page (remove the unsafe parts).
        /// </summary>
        FilterAndSanitize,

        /// <summary>
        /// Enables XSS filtering. Rather than sanitizing the page, the browser will prevent rendering of 
        /// the page if an attack is detected.
        /// </summary>
        FilterAndBlock
    }
}
