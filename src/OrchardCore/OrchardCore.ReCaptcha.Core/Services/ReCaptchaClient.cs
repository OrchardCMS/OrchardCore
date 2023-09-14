using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.ReCaptcha.Configuration;

namespace OrchardCore.ReCaptcha.Services
{
    public class ReCaptchaClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ReCaptchaSettings _reCaptchaSettings;
        private readonly ILogger _logger;

        public ReCaptchaClient(
            IHttpClientFactory httpClientFactory,
            IOptions<ReCaptchaSettings> reCaptchaSettings,
            ILogger<ReCaptchaClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _reCaptchaSettings = reCaptchaSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Verifies the supplied token with ReCaptcha Api.
        /// </summary>
        /// <param name="responseToken">Token received from the ReCaptcha UI.</param>
        /// <returns>A boolean indicating if the token is valid.</returns>
        public async Task<bool> VerifyAsync(string responseToken)
        {
            if (string.IsNullOrWhiteSpace(responseToken) || !_reCaptchaSettings.IsValid())
            {
                return false;
            }

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "secret", _reCaptchaSettings.SecretKey },
                { "response", responseToken }
            });
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsync(_reCaptchaSettings.ReCaptchaApiUri.TrimEnd('/') + "/siteverify", content);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseModel = JObject.Parse(responseJson);

                return responseModel["success"].Value<bool>();
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Could not contact Google to verify captcha.");
            }

            return false;
        }
    }
}
