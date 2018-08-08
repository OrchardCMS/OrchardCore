using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.ReCaptcha.Core.Configuration;

namespace OrchardCore.ReCaptcha.Core.Services
{
    public class ReCaptchaClient : IReCaptchaClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ReCaptchaClient> _logger;

        public ReCaptchaClient(HttpClient httpClient, ILogger<ReCaptchaClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

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
                var response = await _httpClient.PostAsync("siteverify", content);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseModel = JObject.Parse(responseJson);

                return responseModel["success"].Value<bool>();
            }
            catch (HttpRequestException  e)
            {
                _logger.LogError(e, "Could not contact Google to verify captcha");
            }

            return false;
        }
    }
}
