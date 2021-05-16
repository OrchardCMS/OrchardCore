using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
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

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!_authenticationOptions.Value.SchemeMap.ContainsKey(Options.ApiAuthenticationScheme))
            {
                return Task.FromResult<AuthenticateResult>(AuthenticateResult.NoResult());
            }

            return Context.AuthenticateAsync(Options.ApiAuthenticationScheme);
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            if (!_authenticationOptions.Value.SchemeMap.ContainsKey(Options.ApiAuthenticationScheme))
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                return Task.CompletedTask;
            }

            var statusCodePagesFeature = Context.Features.Get<IStatusCodePagesFeature>();
            if (statusCodePagesFeature != null)
            {
                statusCodePagesFeature.Enabled = false;
            }

            return Context.ChallengeAsync(Options.ApiAuthenticationScheme);
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            if (!_authenticationOptions.Value.SchemeMap.ContainsKey(Options.ApiAuthenticationScheme))
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return Task.CompletedTask;
            }

            var statusCodePagesFeature = Context.Features.Get<IStatusCodePagesFeature>();
            if (statusCodePagesFeature != null)
            {
                statusCodePagesFeature.Enabled = false;
            }
            
            return Context.ForbidAsync(Options.ApiAuthenticationScheme);
        }
    }
}
