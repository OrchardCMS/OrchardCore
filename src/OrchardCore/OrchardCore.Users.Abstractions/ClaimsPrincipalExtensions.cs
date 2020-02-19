using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace OrchardCore.Users
{
    /// <summary>
    /// Provides extension methods for <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets a serialized version of the claims.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>A list of <see cref="SerializableClaim"/>.</returns>
        public static IEnumerable<SerializableClaim> GetSerializableClaims(this ClaimsPrincipal principal)
        {
            return principal.Claims.Select(c => new SerializableClaim
            {
                Subject = c.Subject.Name,
                Issuer = c.Issuer,
                OriginalIssuer = c.OriginalIssuer,
                Properties = c.Properties.ToArray(),
                Type = c.Type,
                Value = c.Value,
                ValueType = c.ValueType
            });
        }
    }
}
