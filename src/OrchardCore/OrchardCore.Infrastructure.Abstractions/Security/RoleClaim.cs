using System.Security.Claims;

namespace OrchardCore.Security
{
    /// <summary>
    /// Represents a claim that is granted to all users within a role.
    /// </summary>
    public class RoleClaim
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

        public static implicit operator Claim(RoleClaim claim)
        {
            return claim.ToClaim();
        }
    }
}
