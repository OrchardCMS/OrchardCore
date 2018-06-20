using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Forms.Configuration;

namespace OrchardCore.Forms.Services
{
    public class NoCaptchaClient
    {
        private readonly HttpClient _httpClient;
        private readonly NoCaptchaSettings _settings;

        public NoCaptchaClient(HttpClient httpClient, IOptions<NoCaptchaSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<bool> VerifyAsync(string responseToken)
        {
            if (string.IsNullOrWhiteSpace(responseToken))
            {
                return false;
            }

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "secret", _settings.SiteSecret },
                { "response", responseToken }
            });
            var response = await _httpClient.PostAsync("", content);

            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var responseModel = JObject.Parse(responseJson);

            return responseModel["success"].Value<bool>();
        }
    }
}
