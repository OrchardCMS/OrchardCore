using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Twitter.Services
{
    public class TwitterClient
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public TwitterClient(HttpClient client, ILogger<TwitterClient> logger)
        {
            _client = client;
            _client.BaseAddress = new Uri("https://api.twitter.com");
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _logger = logger;
        }

        public async Task<HttpResponseMessage> UpdateStatus(string status, params string[] optionalParameters)
        {
            try
            {
                var parameters = new Dictionary<string, string>
                {
                    { "status", status }
                };

                if (optionalParameters is not null)
                {
                    for (var i = 0; i < optionalParameters.Length; i++)
                    {
                        var optionalParameter = optionalParameters[i];
                        var parts = optionalParameter.Split('=', 2);
                        if (parts.Length != 2)
                        {
                            _logger.LogWarning("Parameter {Parameter} ignored, has wrong format", optionalParameter);
                            continue;
                        }
                        parameters.Add(parts[0], parts[1]);
                    }
                }

                var content = new FormUrlEncodedContent(parameters);
                var uri = new Uri("/1.1/statuses/update.json", UriKind.Relative);
                var response = await _client.PostAsync(uri, content);
                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "An error occurred connecting to Twitter API");
                throw;
            }
        }
    }
}
