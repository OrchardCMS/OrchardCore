using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using OrchardCore.Users.Handlers;

namespace OrchardCore.Users
{
    public static class ClaimsPrincipalExtensions
    {
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
