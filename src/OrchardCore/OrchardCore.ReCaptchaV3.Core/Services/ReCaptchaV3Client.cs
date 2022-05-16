using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.ReCaptchaV3.Configuration;

namespace OrchardCore.ReCaptchaV3.Services
{
    public class ReCaptchaV3Client
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public ReCaptchaV3Client(HttpClient httpClient, IOptions<ReCaptchaV3Settings> optionsAccessor, ILogger<ReCaptchaV3Client> logger)
        {
            var options = optionsAccessor.Value;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.ReCaptchaV3ApiUri);
            _logger = logger;
        }

        /// <summary>
        /// Verifies the supplied token with ReCaptcha Api
        /// </summary>
        /// <param name="responseToken">Token received from the ReCaptcha UI</param>
        /// <param name="secretKey">Key entered by user in the secrets</param>
        /// <param name="threshold">Threshold value between 0.0 - 1.0</param>
        /// <returns>A boolean indicating if the token is valid</returns>
        public async Task<bool> VerifyAsync(string responseToken, string secretKey, decimal threshold)
        {
            if (string.IsNullOrWhiteSpace(responseToken))
            {
                return false;
            }

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "secret", secretKey },
                { "response", responseToken }
            });
            try
            {
                var response = await _httpClient.PostAsync("siteverify", content);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseModel = JObject.Parse(responseJson);

                return responseModel["success"].Value<bool>() && responseModel["score"].Value<decimal>() >= threshold;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Could not contact Google to verify captcha");
            }

            return false;
        }
    }
}
