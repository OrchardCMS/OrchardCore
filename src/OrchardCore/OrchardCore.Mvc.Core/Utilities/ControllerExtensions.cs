using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Mvc.Utilities
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Returns the proper actionresult for unauthorized or unauthenticated users.
        /// Will return a forbid when the user is authenticated.
        /// Will return a challenge when the user is not authenticated.
        /// If a authentication scheme is specified, will return a challenge to it.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="authenticationScheme"></param>
        /// <returns>The proper actionresult based upon if the user is authenticated</returns>
        public static ActionResult ChallengeOrForbid(this Controller controller, string authenticationScheme = null)
        {
            if (!string.IsNullOrEmpty(authenticationScheme))
            {
                return controller.User.Identity.IsAuthenticated ? (ActionResult)controller.Forbid(new string[] { authenticationScheme }) : (ActionResult)controller.Challenge(new string[] { authenticationScheme });
            }

            return controller.User.Identity.IsAuthenticated ? (ActionResult)controller.Forbid() : (ActionResult)controller.Challenge();
        }
    }
}
