using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ReCaptcha.Services
{
    public class ReCaptchaClient
    {
        public const string Name = "ReCaptcha";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public ReCaptchaClient(
            IHttpClientFactory httpClientFactory,
            ILogger<ReCaptchaClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Verifies the supplied token with ReCaptcha Api.
        /// </summary>
        /// <param name="responseToken">Token received from the ReCaptcha UI.</param>
        /// <param name="secretKey">Key entered by user in the secrets.</param>
        /// <returns>A boolean indicating if the token is valid.</returns>
        public async Task<bool> VerifyAsync(string responseToken, string secretKey)
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
                var client = _httpClientFactory.CreateClient(Name);

                var response = await client.PostAsync("siteverify", content);
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
