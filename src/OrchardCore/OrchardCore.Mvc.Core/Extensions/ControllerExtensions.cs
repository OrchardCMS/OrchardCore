using System;

namespace Microsoft.AspNetCore.Mvc
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Returns the proper actionresult for unauthorized or unauthenticated users.
        /// Will return a forbid when the user is authenticated.
        /// Will return a challenge when the user is not authenticated.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>The proper actionresult based upon if the user is authenticated</returns>
        public static ActionResult ChallengeOrForbid(this Controller controller)
            => controller.User?.Identity?.IsAuthenticated ?? false ? (ActionResult)controller.Forbid() : controller.Challenge();

        /// <summary>
        /// Returns the proper actionresult for unauthorized or unauthenticated users
        /// with the specified authenticationSchemes.
        /// Will return a forbid when the user is authenticated.
        /// Will return a challenge when the user is not authenticated.
        /// If authentication schemes are specified, will return a challenge to them.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="authenticationSchemes">The authentication schemes to challenge.</param>
        /// <returns>The proper actionresult based upon if the user is authenticated</returns>
        public static ActionResult ChallengeOrForbid(this Controller controller, params string[] authenticationSchemes)
            => controller.User?.Identity?.IsAuthenticated ?? false ? (ActionResult)controller.Forbid(authenticationSchemes) : controller.Challenge(authenticationSchemes);

        /// <summary>
        /// Creates a <see cref="LocalRedirectResult"/> object that redirects to the specified local localUrl
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="localUrl">The local URL to redirect to.</param>
        /// <param name="escapeUrl">Whether to escape the url.</param>
        public static ActionResult LocalRedirect(this Controller controller, string localUrl, bool escapeUrl)
        {
            return controller.LocalRedirect(EscapeLocationHeader(localUrl));
        }


        /// <summary>
        /// Creates a <see cref="RedirectResult"/> object that redirects to the specified url
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="url">The local URL to redirect to.</param>
        /// <param name="escapeUrl">Whether to escape the url.</param>
        public static ActionResult Redirect(this Controller controller, string url, bool escapeUrl)
        {
            return controller.Redirect(EscapeLocationHeader(url));
        }

        public static string EscapeLocationHeader(string stringToEscape)
        {
            if (String.IsNullOrEmpty(stringToEscape))
            {
                return stringToEscape;
            }

            var uri = new Uri(stringToEscape, UriKind.RelativeOrAbsolute);
            return uri.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);
        }
    }
}
