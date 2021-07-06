using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Mvc.Utilities
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
    }
}
