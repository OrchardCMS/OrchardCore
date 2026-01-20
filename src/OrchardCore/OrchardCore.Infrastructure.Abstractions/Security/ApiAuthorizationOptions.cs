using Microsoft.AspNetCore.Authentication;

namespace OrchardCore.Security
{
    public class ApiAuthorizationOptions : AuthenticationSchemeOptions
    {
        public string ApiAuthenticationScheme { get; set; } = "Bearer";
    }
}
