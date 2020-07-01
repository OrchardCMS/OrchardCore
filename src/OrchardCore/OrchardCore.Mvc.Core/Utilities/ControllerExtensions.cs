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
        {
            return controller.User.Identity.IsAuthenticated ? (ActionResult)controller.Forbid() : (ActionResult)controller.Challenge();
        }
    }
}
