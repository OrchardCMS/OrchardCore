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
    public class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthorizationOptions>
    {
        private readonly IOptions<AuthenticationOptions> _authenticationOptions;

        public ApiAuthenticationHandler(
            IOptions<AuthenticationOptions> authenticationOptions,
            IOptionsMonitor<ApiAuthorizationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _authenticationOptions = authenticationOptions;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!_authenticationOptions.Value.SchemeMap.ContainsKey(Options.ApiAuthenticationScheme))
            {
                return AuthenticateResult.NoResult();
            }

            return await Context.AuthenticateAsync(Options.ApiAuthenticationScheme);
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            if (!_authenticationOptions.Value.SchemeMap.ContainsKey(Options.ApiAuthenticationScheme))
            {
                return Task.CompletedTask;
            }

            return Context.ChallengeAsync(Options.ApiAuthenticationScheme);
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            if (!_authenticationOptions.Value.SchemeMap.ContainsKey(Options.ApiAuthenticationScheme))
            {
                return Task.CompletedTask;
            }

            return Context.ForbidAsync(Options.ApiAuthenticationScheme);
        }
    }

    public class ApiAuthorizationOptions : AuthenticationSchemeOptions
    {
        public string ApiAuthenticationScheme { get; set; } = "Bearer";
    }
}
