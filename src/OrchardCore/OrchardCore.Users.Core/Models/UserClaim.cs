using System.Security.Claims;

namespace OrchardCore.Users.Models
{
    /// <summary>
    /// Represents a claim that is granted to a specific user.
    /// </summary>
    public class UserClaim
    {
        /// <summary>
        /// Gets or sets the claim type for this claim.
        /// </summary>
        public string ClaimType { get; set; }

        /// <summary>
        /// Gets or sets the claim value for this claim.
        /// </summary>
        public string ClaimValue { get; set; }

        public Claim ToClaim()
        {
            return new Claim(ClaimType, ClaimValue);
        }
    }
}
