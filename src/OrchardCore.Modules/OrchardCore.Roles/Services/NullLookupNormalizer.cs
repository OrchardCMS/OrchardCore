using Microsoft.AspNetCore.Identity;

namespace OrchardCore.Roles.Services
{
    public class NullLookupNormalizer : ILookupNormalizer
    {
        public string NormalizeEmail(string email)
        {
            return email;
        }

        public string NormalizeName(string name)
        {
            return name;
        }
    }
}
