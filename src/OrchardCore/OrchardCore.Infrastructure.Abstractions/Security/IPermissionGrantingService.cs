using System.Collections.Generic;
using System.Security.Claims;

namespace OrchardCore.Security
{
    public interface IPermissionGrantingService
    {
        /// <summary>
        /// Evaluates if the specified <see cref="PermissionRequirement"/> is granted by provided claims.
        /// </summary>
        /// <param name="requirement">The <see cref="PermissionRequirement"/> to challenge</param>
        /// <param name="claims">Provided claims.</param>
        /// <returns>True if the permission is granted, otherwise false.</returns>
        public bool IsGranted(PermissionRequirement requirement, IEnumerable<Claim> claims);
    }
}
