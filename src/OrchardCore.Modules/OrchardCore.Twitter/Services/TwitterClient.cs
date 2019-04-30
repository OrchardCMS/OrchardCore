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

namespace OrchardCore.Twitter.Services
{
    public class TwitterClient
    {
        internal HttpClient _client;
        internal ILogger<TwitterClient> _logger;
        internal IOptionsMonitor<TwitterClientOptions> _clientOptionsMonitor;
        internal IOptionsMonitor<TwitterOptions> _optionsMonitor;
        internal IClock _clock;

        public TwitterClient(HttpClient client, IClock clock, ILogger<TwitterClient> logger, IOptionsMonitor<TwitterOptions> optionsMonitor, IOptionsMonitor<TwitterClientOptions> clientOptionsMonitor)
        {
            _client = client;
            _client.BaseAddress = new Uri($"https://api.twitter.com");
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _clock = clock;

            _optionsMonitor = optionsMonitor;
            _clientOptionsMonitor = clientOptionsMonitor;

            _logger = logger;
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

                content.Headers.Add("Authentication", await GetOauthHeader("POST", uri, parameters));

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

        public Task<string> GetOauthHeader(string method, Uri uri, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var options = _optionsMonitor.CurrentValue;
            var clientOptions = _clientOptionsMonitor.CurrentValue;

            var nonce = GetNonce();
            var timeStamp = Convert.ToInt64((_clock.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();

            var sortedParameters = new SortedDictionary<string, string>();

            sortedParameters.Add(Uri.EscapeDataString("oauth_consumer_key"), Uri.EscapeDataString(options.ConsumerKey));
            sortedParameters.Add(Uri.EscapeDataString("oauth_nonce"), Uri.EscapeDataString(nonce));
            sortedParameters.Add(Uri.EscapeDataString("oauth_signature_method"), Uri.EscapeDataString("HMAC-SHA1"));
            sortedParameters.Add(Uri.EscapeDataString("oauth_timestamp"), Uri.EscapeDataString(timeStamp));
            sortedParameters.Add(Uri.EscapeDataString("oauth_token"), Uri.EscapeDataString(clientOptions.AccessToken));
            sortedParameters.Add(Uri.EscapeDataString("oauth_version"), Uri.EscapeDataString("1.0"));

            foreach (var item in parameters)
            {
                sortedParameters.Add(Uri.EscapeDataString(item.Key), Uri.EscapeDataString(item.Value));
            }

            var baseString = string.Concat(method.ToUpperInvariant(), "&",
                Uri.EscapeDataString(new Uri(_client.BaseAddress, uri).ToString()), "&",
                Uri.EscapeDataString(string.Join("&", sortedParameters.Select(c => string.Format("{0}={1}", c.Key, c.Value)))));

            var secret = string.Concat(options.ConsumerSecret, "&", clientOptions.AccessTokenSecret);
            string signature;
            using (var hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(secret)))
            {
                signature = Convert.ToBase64String(hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
            }

            var sb = new StringBuilder("OAuth ");
            sb.Append($"oauth_consumer_key=\"{Uri.EscapeDataString(options.ConsumerKey)}\", ");
            sb.Append($"oauth_nonce=\"{Uri.EscapeDataString(nonce)}\", ");
            sb.Append($"oauth_signature=\"{Uri.EscapeDataString(signature)}\", ");
            sb.Append($"oauth_signature_method=\"HMAC-SHA1\", ");
            sb.Append($"oauth_timestamp=\"{Uri.EscapeDataString(timeStamp)}\", ");
            sb.Append($"oauth_token=\"{Uri.EscapeDataString(clientOptions.AccessToken)}\", ");
            sb.Append($"oauth_version=\"{Uri.EscapeDataString("1.0")}\"");

            return Task.FromResult(sb.ToString());
        }

    }
}
