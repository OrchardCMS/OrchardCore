using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.ReCaptcha.ActionFilters.Detection;
using OrchardCore.ReCaptcha.Configuration;

namespace OrchardCore.ReCaptcha.Services
{
    public class ReCaptchaService
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
        };

        private readonly ReCaptchaSettings _reCaptchaSettings;
        private readonly HttpClient _httpClient;
        private readonly IEnumerable<IDetectRobots> _robotDetectors;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        protected readonly IStringLocalizer S;

        public ReCaptchaService(
            HttpClient httpClient,
            IOptions<ReCaptchaSettings> optionsAccessor,
            IEnumerable<IDetectRobots> robotDetectors,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ReCaptchaService> logger,
            IStringLocalizer<ReCaptchaService> stringLocalizer)
        {
            _reCaptchaSettings = optionsAccessor.Value;
            _httpClient = httpClient;
            _robotDetectors = robotDetectors;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            S = stringLocalizer;
        }

        /// <summary>
        /// Flags the behavior as that of a robot.
        /// </summary>
        public void MaybeThisIsARobot()
            => _robotDetectors.Invoke(i => i.FlagAsRobot(), _logger);

        /// <summary>
        /// Determines if the request has been made by a robot.
        /// </summary>
        /// <returns>Yes (true) or no (false).</returns>
        public bool IsThisARobot()
            => _robotDetectors.Invoke(i => i.DetectRobot(), _logger)
            .Any(a => a.IsRobot);

        /// <summary>
        /// Clears all robot markers, we are dealing with a human.
        /// </summary>
        /// <returns></returns>
        public void ThisIsAHuman()
            => _robotDetectors.Invoke(i => i.IsNotARobot(), _logger);

        /// <summary>
        /// Verifies the ReCaptcha response with the ReCaptcha webservice.
        /// </summary>
        /// <param name="reCaptchaResponse"></param>
        /// <returns></returns>
        public async Task<bool> VerifyCaptchaResponseAsync(string reCaptchaResponse)
            => !string.IsNullOrWhiteSpace(reCaptchaResponse)
                && _reCaptchaSettings.IsValid()
                && await VerifyAsync(reCaptchaResponse);


        /// <summary>
        /// Validates the captcha that is in the Form of the current request.
        /// </summary>
        /// <param name="reportError">Lambda for reporting errors.</param>
        public async Task<bool> ValidateCaptchaAsync(Action<string, string> reportError)
        {
            if (!_reCaptchaSettings.IsValid())
            {
                _logger.LogWarning("The ReCaptcha settings are invalid");
                return false;
            }

            // We use the header value as default if it's passed
            var reCaptchaResponse = _httpContextAccessor.HttpContext?.Request.Headers[Constants.ReCaptchaServerResponseHeaderName];

            // If this is a standard form post we get the token from the form values if not affected previously in the header.
            if (string.IsNullOrEmpty(reCaptchaResponse) && (_httpContextAccessor.HttpContext?.Request.HasFormContentType ?? false))
            {
                reCaptchaResponse = _httpContextAccessor.HttpContext.Request.Form[Constants.ReCaptchaServerResponseHeaderName].ToString();
            }

            var isValid = await VerifyCaptchaResponseAsync(reCaptchaResponse);

            if (!isValid)
            {
                reportError("ReCaptcha", S["Failed to validate ReCaptcha"]);
            }

            return isValid;
        }

        /// <summary>
        /// Verifies the supplied token with ReCaptcha Api.
        /// </summary>
        /// <param name="responseToken">Token received from the ReCaptcha UI.</param>
        /// <returns>A boolean indicating if the token is valid.</returns>
        private async Task<bool> VerifyAsync(string responseToken)
        {
            try
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "secret", _reCaptchaSettings.SecretKey },
                    { "response", responseToken }
                });

                var response = await _httpClient.PostAsync($"{_reCaptchaSettings.ReCaptchaApiUri.TrimEnd('/')}/siteverify", content);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<ReCaptchaResponse>(_jsonSerializerOptions);

                return result.Success;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Could not contact Google to verify ReCaptcha.");
            }

            return false;
        }
    }
}
