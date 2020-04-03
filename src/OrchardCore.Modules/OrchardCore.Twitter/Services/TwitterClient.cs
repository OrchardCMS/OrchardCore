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

        public TwitterClient(HttpClient client, ILogger<TwitterClient> logger)
        {
            _client = client;
            _client.BaseAddress = new Uri($"https://api.twitter.com");
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
                    for (int i = 0; i < optionalParameters.Length; i++)
                    {
                        var optionalParameter = optionalParameters[i];
                        var parts = optionalParameter.Split('=');
                        if (parts.Length != 2)
                        {
                            _logger.LogWarning("Parameter {optionalParameter} ignored, has wrong format", optionalParameter);
                            continue;
                        }
                        parameters.Add(parts[0], parts[1]);
                    }
                }

                var content = new FormUrlEncodedContent(parameters);
                var uri = new Uri($"/1.1/statuses/update.json", UriKind.Relative);
                var response = await _client.PostAsync(uri, content);
                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occurred connecting to Twitter API {ex.ToString()}");
                throw;
            }
        }
    }
}
