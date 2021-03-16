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
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Services
{
    public class TwitterClientMessageHandler : DelegatingHandler
    {
        private readonly ISiteService _siteService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IClock _clock;

        public TwitterClientMessageHandler(IClock clock, ISiteService siteService, IDataProtectionProvider dataProtectionProvider)
        {
            _siteService = siteService;
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
            var container = await _siteService.GetSiteSettingsAsync();
            var settings = container.As<TwitterSettings>();
            var protrector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter);
            var queryString = request.RequestUri.Query;

            if (!string.IsNullOrWhiteSpace(settings.ConsumerSecret))
                settings.ConsumerSecret = protrector.Unprotect(settings.ConsumerSecret);
            if (!string.IsNullOrWhiteSpace(settings.ConsumerSecret))
                settings.AccessTokenSecret = protrector.Unprotect(settings.AccessTokenSecret);

            var nonce = GetNonce();
            var timeStamp = Convert.ToInt64((_clock.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();

            var sortedParameters = new SortedDictionary<string, string>();

            sortedParameters.Add(Uri.EscapeDataString("oauth_consumer_key"), Uri.EscapeDataString(settings.ConsumerKey));
            sortedParameters.Add(Uri.EscapeDataString("oauth_nonce"), Uri.EscapeDataString(nonce));
            sortedParameters.Add(Uri.EscapeDataString("oauth_signature_method"), Uri.EscapeDataString("HMAC-SHA1"));
            sortedParameters.Add(Uri.EscapeDataString("oauth_timestamp"), Uri.EscapeDataString(timeStamp));
            sortedParameters.Add(Uri.EscapeDataString("oauth_token"), Uri.EscapeDataString(settings.AccessToken));
            sortedParameters.Add(Uri.EscapeDataString("oauth_version"), Uri.EscapeDataString("1.0"));

            if (!string.IsNullOrEmpty(request.RequestUri.Query))
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

            if (!string.IsNullOrEmpty(contentString))
            {
                foreach (var item in contentString.Split('&'))
                {
                    var parts = item.Split('=');

                    var key = Uri.EscapeDataString(WebUtility.UrlDecode(parts[0]));
                    var value = Uri.EscapeDataString(WebUtility.UrlDecode(parts[1]));
                    sortedParameters.Add(key, value);
                }
            }

            var baseString = string.Concat(request.Method.Method.ToUpperInvariant(), "&",
                Uri.EscapeDataString(request.RequestUri.AbsoluteUri.ToString()), "&",
                Uri.EscapeDataString(string.Join("&", sortedParameters.Select(c => string.Format("{0}={1}", c.Key, c.Value)))));

            var secret = string.Concat(settings.ConsumerSecret, "&", settings.AccessTokenSecret);
            string signature;
            using (var hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(secret)))
            {
                signature = Convert.ToBase64String(hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
            }

            var sb = new StringBuilder();
            sb.Append("oauth_consumer_key=\"").Append(Uri.EscapeDataString(settings.ConsumerKey)).Append("\", ");
            sb.Append("oauth_nonce=\"").Append(Uri.EscapeDataString(nonce)).Append("\", ");
            sb.Append("oauth_signature=\"").Append(Uri.EscapeDataString(signature)).Append("\", ");
            sb.Append($"oauth_signature_method=\"HMAC-SHA1\", ");
            sb.Append("oauth_timestamp=\"").Append(Uri.EscapeDataString(timeStamp)).Append("\", ");
            sb.Append("oauth_token=\"").Append(Uri.EscapeDataString(settings.AccessToken)).Append("\", ");
            sb.Append("oauth_version=\"").Append(Uri.EscapeDataString("1.0")).Append('"');

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", sb.ToString());
        }
    }
}
