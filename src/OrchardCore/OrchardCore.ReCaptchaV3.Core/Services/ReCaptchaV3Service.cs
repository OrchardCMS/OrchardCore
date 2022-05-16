using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ReCaptchaV3.Configuration;

namespace OrchardCore.ReCaptchaV3.Services
{
    public class ReCaptchaV3Service
    {
        private readonly ReCaptchaV3Client _reCaptchaV3Client;
        private readonly ReCaptchaV3Settings _settings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly IStringLocalizer S;

        public ReCaptchaV3Service(ReCaptchaV3Client reCaptchaV3Client, IOptions<ReCaptchaV3Settings> optionsAccessor, IHttpContextAccessor httpContextAccessor, ILogger<ReCaptchaV3Service> logger, IStringLocalizer<ReCaptchaV3Service> stringLocalizer)
        {
            _reCaptchaV3Client = reCaptchaV3Client;
            _settings = optionsAccessor.Value;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            S = stringLocalizer;
        }

        /// <summary>
        /// Verifies the ReCaptcha response with the ReCaptcha webservice
        /// </summary>
        /// <param name="reCaptchaV3Response"></param>
        /// <returns></returns>
        public async Task<bool> VerifyCaptchaV3ResponseAsync(string reCaptchaV3Response)
        {
            return !String.IsNullOrWhiteSpace(reCaptchaV3Response) && await _reCaptchaV3Client.VerifyAsync(reCaptchaV3Response, _settings.SecretKey, _settings.Threshold);
        }

        /// <summary>
        /// Validates the captcha that is in the Form of the current request
        /// </summary>
        /// <param name="reportError">Lambda for reporting errors</param>
        public async Task<bool> ValidateCaptchaV3Async(Action<string, string> reportError)
        {
            if (!_settings.IsValid())
            {
                _logger.LogWarning("The ReCaptchaV3 settings are not valid");
                return false;
            }

            // We use the header value as default if it's passed
            var reCaptchaV3Response = _httpContextAccessor.HttpContext?.Request.Headers[Constants.ReCaptchaV3ServerResponseHeaderName];

            // If this is a standard form post we get the token from the form values if not affected previously in the header
            if (String.IsNullOrEmpty(reCaptchaV3Response) && (_httpContextAccessor.HttpContext?.Request.HasFormContentType ?? false))
            {
                reCaptchaV3Response = _httpContextAccessor.HttpContext.Request.Form[Constants.ReCaptchaV3ServerResponseHeaderName].ToString();
            }

            var isValid = !String.IsNullOrEmpty(reCaptchaV3Response) && await VerifyCaptchaV3ResponseAsync(reCaptchaV3Response);

            if (!isValid)
            {
                reportError("ReCaptcha", S["Failed to validate captchaV3"]);
            }

            return isValid;
        }
    }
}
