namespace OrchardCore.Security.SecurityHeaders
{
    /// <summary>
    /// The referrer policy for the security header.
    /// </summary>
    /// <remarks>
    /// See: https://w3c.github.io/webappsec-referrer-policy/#referrer-policy-header
    /// </remarks>
    public enum ReferrerPolicy
    {
		/// <summary>
		/// The Referer header will be omitted entirely from browser requests. No referrer information is sent along with requests.
        /// </summary>
        NoReferrer,

        /// <summary>
        /// This is the user agent's default behavior if no policy is specified. The origin is sent as a referrer 
        /// when the protocol security level stays the same (HTTPS->HTTPS), but isn't sent to a less secure 
        /// destination (HTTPS->HTTP).
        /// </summary>
        NoReferrerWhenDowngrade,

        /// <summary>
        /// Only send the origin of the document as the referrer in all cases.
        /// The document https://example.com/page.html will send the referrer https://example.com/.
        /// </summary>
        Origin,

        /// <summary>
        /// Send a full URL when performing a same-origin request, but only send the origin of the document for other cases.
        /// </summary>
        OriginWhenCrossOrigin,

		/// <summary>
        /// A referrer will be sent for same-site origins, but cross-origin requests will contain no referrer information.
        /// </summary>
        SameOrigin,

        /// <summary>
        /// Only send the origin of the document as the referrer to a-priori as-much-secure destination (HTTPS->HTTPS), 
        /// but don't send it to a less secure destination (HTTPS->HTTP).
        /// </summary>
        StrictOrigin,

        /// <summary>
        /// Send a full URL when performing a same-origin request, only send the origin of the document to a-priori as-much-secure destination (HTTPS->HTTPS), 
        /// and send no header to a less secure destination (HTTPS->HTTP).
        /// </summary>
        StrictOriginWhenCrossOrigin,

        /// <summary>
        /// Send a full URL when performing a same-origin or cross-origin request.
        /// This policy will leak origins and paths from TLS-protected resources to insecure origins. Carefully consider the impact of this setting.
        /// </summary>
        UnsafeUrl
    }
}
