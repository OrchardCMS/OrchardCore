using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Forms.Configuration;

namespace OrchardCore.Forms.Services
{
    public class ReCaptchaClient : IReCaptchaClient
    {
#pragma warning disable S1075 // URIs should not be hardcoded. Justification: if the URL is changed by Google, odds are that there are other changes we will need to make as well other than just the URL.
        private const string SiteVerifyUrl = "https://www.google.com/recaptcha/api/siteverify";
#pragma warning restore S1075 // URIs should not be hardcoded.
        private readonly ReCaptchaSettings _settings;
        private readonly HttpClient _httpClient;

        public ReCaptchaClient(IOptions<ReCaptchaSettings> settings)
        {
            _settings = settings.Value;

            // TODO: Refactor this using HttpClientFactory with Polly policies when we're on .NET Core 2.1. For now, we'll create an instance here.
            _httpClient = new HttpClient();
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
            var response = await _httpClient.PostAsync(SiteVerifyUrl, content);

            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var responseModel = JObject.Parse(responseJson);

            return responseModel["success"].Value<bool>();
        }
    }
}
