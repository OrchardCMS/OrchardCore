using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Security
{
    /// <summary>
    /// Provides a delegating logic for API authentication.
    /// If no specific scheme handler is found it returns an anonymous user.
    /// </summary>
    public class ApiAuthorizationHandler : AuthenticationHandler<ApiAuthorizationOptions>
    {
        private readonly IOptions<AuthenticationOptions> _authenticationOptions;
        private readonly IAuthenticationService _authenticationService;
        private static readonly AuthenticateResult Anonymous;

        static ApiAuthorizationHandler()
        {
            Anonymous = AuthenticateResult.Success(
                new AuthenticationTicket(
                    new ClaimsPrincipal(
                        new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Anonymous") }, "Bearer")), "Bearer")
                );
        }

        public ApiAuthorizationHandler(
            IOptions<AuthenticationOptions> authenticationOptions,
            IAuthenticationService authenticationService,
            IOptionsMonitor<ApiAuthorizationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _authenticationOptions = authenticationOptions;
            _authenticationService = authenticationService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!_authenticationOptions.Value.SchemeMap.ContainsKey("Bearer"))
            {
                return Anonymous;
            }

            try
            {
                return await Context.AuthenticateAsync("Bearer");
            }
            catch
            {
                return Anonymous;
            }
        }
    }

    public class ApiAuthorizationOptions : AuthenticationSchemeOptions
    {
    }
}