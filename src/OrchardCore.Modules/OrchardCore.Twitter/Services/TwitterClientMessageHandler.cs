using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Services
{
    public class TwitterClientMessageHandler : DelegatingHandler
    {
        private readonly TwitterSettings _twitterSettings;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IClock _clock;

        public TwitterClientMessageHandler(
            IClock clock,
            IOptions<TwitterSettings> twitterSettings,
            IDataProtectionProvider dataProtectionProvider)
        {
            _twitterSettings = twitterSettings.Value;
            _dataProtectionProvider = dataProtectionProvider;
            _clock = clock;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await ConfigureOAuthAsync(request);
            return await base.SendAsync(request, cancellationToken);
        }

        public virtual string GetNonce()
        {
            return Convert.ToBase64String(new ASCIIEncoding().GetBytes(_clock.UtcNow.Ticks.ToString()));
        }

        public async Task ConfigureOAuthAsync(HttpRequestMessage request)
        {
            var protrector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter);
            var queryString = request.RequestUri.Query;

            if (!String.IsNullOrWhiteSpace(_twitterSettings.ConsumerSecret))
            {
                _twitterSettings.ConsumerSecret = protrector.Unprotect(_twitterSettings.ConsumerSecret);
            }

            if (!String.IsNullOrWhiteSpace(_twitterSettings.AccessTokenSecret))
            {
                _twitterSettings.AccessTokenSecret = protrector.Unprotect(_twitterSettings.AccessTokenSecret);
            }

            var nonce = GetNonce();
            var timeStamp = Convert.ToInt64((_clock.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();

            var sortedParameters = new SortedDictionary<string, string>
            {
                { Uri.EscapeDataString("oauth_consumer_key"), Uri.EscapeDataString(_twitterSettings.ConsumerKey) },
                { Uri.EscapeDataString("oauth_nonce"), Uri.EscapeDataString(nonce) },
                { Uri.EscapeDataString("oauth_signature_method"), Uri.EscapeDataString("HMAC-SHA1") },
                { Uri.EscapeDataString("oauth_timestamp"), Uri.EscapeDataString(timeStamp) },
                { Uri.EscapeDataString("oauth_token"), Uri.EscapeDataString(_twitterSettings.AccessToken) },
                { Uri.EscapeDataString("oauth_version"), Uri.EscapeDataString("1.0") },
            };

            if (!String.IsNullOrEmpty(request.RequestUri.Query))
            {
                foreach (var item in request.RequestUri.Query.Split('&'))
                {
                    var parts = item.Split('=');
                    var key = Uri.EscapeDataString(parts[0]);
                    var value = Uri.EscapeDataString(parts[1]);
                    sortedParameters.Add(key, value);
                }
            }

            var contentString = await request.Content.ReadAsStringAsync();

            if (!String.IsNullOrEmpty(contentString))
            {
                foreach (var item in contentString.Split('&'))
                {
                    var parts = item.Split('=');

                    var key = Uri.EscapeDataString(WebUtility.UrlDecode(parts[0]));
                    var value = Uri.EscapeDataString(WebUtility.UrlDecode(parts[1]));
                    sortedParameters.Add(key, value);
                }
            }

            var baseString = String.Concat(request.Method.Method.ToUpperInvariant(), "&",
                Uri.EscapeDataString(request.RequestUri.AbsoluteUri.ToString()), "&",
                Uri.EscapeDataString(String.Join("&", sortedParameters.Select(c => String.Format("{0}={1}", c.Key, c.Value)))));

            var secret = String.Concat(_twitterSettings.ConsumerSecret, "&", _twitterSettings.AccessTokenSecret);

            string signature;
            using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(secret)))
            {
                signature = Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString)));
            }

            var sb = new StringBuilder();
            sb.Append("oauth_consumer_key=\"").Append(Uri.EscapeDataString(_twitterSettings.ConsumerKey)).Append("\", ");
            sb.Append("oauth_nonce=\"").Append(Uri.EscapeDataString(nonce)).Append("\", ");
            sb.Append("oauth_signature=\"").Append(Uri.EscapeDataString(signature)).Append("\", ");
            sb.Append($"oauth_signature_method=\"HMAC-SHA1\", ");
            sb.Append("oauth_timestamp=\"").Append(Uri.EscapeDataString(timeStamp)).Append("\", ");
            sb.Append("oauth_token=\"").Append(Uri.EscapeDataString(_twitterSettings.AccessToken)).Append("\", ");
            sb.Append("oauth_version=\"").Append(Uri.EscapeDataString("1.0")).Append('"');

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", sb.ToString());
        }
    }
}
