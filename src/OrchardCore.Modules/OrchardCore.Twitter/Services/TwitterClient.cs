using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Twitter.Settings;
using Microsoft.AspNetCore.DataProtection;
using OrchardCore.Entities;

namespace OrchardCore.Twitter.Services
{
    public class TwitterClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<TwitterClient> _logger;
        private readonly IClock _clock;
        private readonly ISiteService _siteService;
        private readonly IDataProtectionProvider _dataProtectionProvider;


        public TwitterClient(HttpClient client, IClock clock, ILogger<TwitterClient> logger, ISiteService siteService, IDataProtectionProvider dataProtectionProvider)
        {
            _client = client;
            _client.BaseAddress = new Uri($"https://api.twitter.com");
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _clock = clock;
            _siteService = siteService;
            _logger = logger;
            _dataProtectionProvider = dataProtectionProvider;
        }

        public async Task<HttpResponseMessage> UpdateStatus(string status, params string[] optionalParameters)
        {
            try
            {
                var parameters = new Dictionary<string, string>();
                parameters.Add("status", status);
                if (optionalParameters is object)
                {
                    for (int i = 0; i < optionalParameters.Length; i += 2)
                    {
                        parameters.Add(optionalParameters[i], optionalParameters[i + 1]);
                    }
                }

                var content = new FormUrlEncodedContent(parameters);
                var uri = new Uri($"/1.1/statuses/update.json", UriKind.Relative);

                var oauthHeader = await GetOauthHeader("POST", uri, parameters);
                content.Headers.Add("Authentication", oauthHeader);
                var response = await _client.PostAsync(uri, content);
                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occurred connecting to Twitter API {ex.ToString()}");
                throw;
            }
        }

        public virtual string GetNonce()
        {
            return Convert.ToBase64String(new ASCIIEncoding().GetBytes(_clock.UtcNow.Ticks.ToString()));
        }

        public async Task<string> GetOauthHeader(string method, Uri uri, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var container = await _siteService.GetSiteSettingsAsync();
            var settings = container.As<TwitterSettings>();
            var protrector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter);

            if (!string.IsNullOrWhiteSpace(settings.ConsumerSecret))
                settings.ConsumerSecret = protrector.Unprotect(settings.ConsumerSecret);
            if (!string.IsNullOrWhiteSpace(settings.ConsumerSecret))
                settings.AccessTokenSecret= protrector.Unprotect(settings.AccessTokenSecret);

            var nonce = GetNonce();
            var timeStamp = Convert.ToInt64((_clock.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();
            var sortedParameters = new SortedDictionary<string, string>();

            sortedParameters.Add(Uri.EscapeDataString("oauth_consumer_key"), Uri.EscapeDataString(settings.ConsumerKey));
            sortedParameters.Add(Uri.EscapeDataString("oauth_nonce"), Uri.EscapeDataString(nonce));
            sortedParameters.Add(Uri.EscapeDataString("oauth_signature_method"), Uri.EscapeDataString("HMAC-SHA1"));
            sortedParameters.Add(Uri.EscapeDataString("oauth_timestamp"), Uri.EscapeDataString(timeStamp));
            sortedParameters.Add(Uri.EscapeDataString("oauth_token"), Uri.EscapeDataString(settings.AccessToken));
            sortedParameters.Add(Uri.EscapeDataString("oauth_version"), Uri.EscapeDataString("1.0"));

            foreach (var item in parameters)
            {
                sortedParameters.Add(Uri.EscapeDataString(item.Key), Uri.EscapeDataString(item.Value));
            }

            var baseString = string.Concat(method.ToUpperInvariant(), "&",
                Uri.EscapeDataString(new Uri(_client.BaseAddress, uri).ToString()), "&",
                Uri.EscapeDataString(string.Join("&", sortedParameters.Select(c => string.Format("{0}={1}", c.Key, c.Value)))));

            var secret = string.Concat(settings.ConsumerSecret, "&", settings.AccessTokenSecret);
            string signature;
            using (var hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(secret)))
            {
                signature = Convert.ToBase64String(hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
            }

            var sb = new StringBuilder("OAuth ");
            sb.Append($"oauth_consumer_key=\"{Uri.EscapeDataString(settings.ConsumerKey)}\", ");
            sb.Append($"oauth_nonce=\"{Uri.EscapeDataString(nonce)}\", ");
            sb.Append($"oauth_signature=\"{Uri.EscapeDataString(signature)}\", ");
            sb.Append($"oauth_signature_method=\"HMAC-SHA1\", ");
            sb.Append($"oauth_timestamp=\"{Uri.EscapeDataString(timeStamp)}\", ");
            sb.Append($"oauth_token=\"{Uri.EscapeDataString(settings.AccessToken)}\", ");
            sb.Append($"oauth_version=\"{Uri.EscapeDataString("1.0")}\"");

            return sb.ToString();
        }

    }
}
