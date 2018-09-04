using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell;
using OrchardCore.WebHooks.Abstractions.Models;
using OrchardCore.WebHooks.Abstractions.Services;
using OrchardCore.WebHooks.Expressions;
using OrchardCore.WebHooks.Models;
using OrchardCore.WebHooks.Services.Http;

namespace OrchardCore.WebHooks.Services
{
    /// <summary>
    /// Provides a base implementation of <see cref="IWebHookSender"/> that defines the default format
    /// for HTTP requests sent as WebHooks.
    /// </summary>
    public class WebHookSender : IWebHookSender
    {
        private const string HeaderPrefix = "X-Orchard-";
        private const string HeaderIdName = HeaderPrefix + "Id";
        private const string HeaderEventName = HeaderPrefix + "Event";
        private const string HeaderTenantName = HeaderPrefix + "Tenant";
        private const string SignatureHeaderName = HeaderPrefix + "Signature";

        private const string SignatureHeaderKey = "sha256";
        private const string SignatureHeaderValueTemplate = SignatureHeaderKey + "={0}";

        private readonly IHttpClientFactory _clientFactory;
        private readonly IWebHookExpressionEvaluator _expressionEvaluator;
        private readonly ShellSettings _shellSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookSender"/> class.
        /// </summary>
        public WebHookSender(
            IHttpClientFactory clientFactory,
            IWebHookExpressionEvaluator expressionEvaluator,
            ShellSettings shellSettings,
            ILogger<WebHookSender> logger)
        {
            Logger = logger;
            _clientFactory = clientFactory;
            _expressionEvaluator = expressionEvaluator;
            _shellSettings = shellSettings;
        }

        public ILogger Logger { get; set; }

        /// <inheritdoc />
        public Task SendNotificationsAsync(IEnumerable<WebHook> webHooks, WebHookNotificationContext context)
        {
            if (webHooks == null) throw new ArgumentNullException(nameof(webHooks));
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Send all webhooks in parallel
            return Task.WhenAll(webHooks.Select(webHook => SendWebHookAsync(webHook, context)));
        }

        private async Task SendWebHookAsync(WebHook webHook, WebHookNotificationContext context)
        {
            try
            {
                // Setup and send WebHook request
                var request = await CreateWebHookRequestAsync(webHook, context);

                var clientName = webHook.ValidateSsl ? "webhooks" : "webhooks_insecure";
                var client = _clientFactory.CreateClient(clientName);

                if (Logger.IsEnabled(LogLevel.Debug))
                {
                    var contentMessage = $"Sending webHook '{webHook.Id}' with body '{await request.Content.ReadAsStringAsync()}'.";
                    Logger.LogDebug(contentMessage);
                }

                var response = await client.SendAsync(request);
                
                var message = $"WebHook '{webHook.Id}' resulted in status code '{response.StatusCode}'.";
                Logger.LogInformation(message);
            }
            catch (Exception ex)
            {
                var message = $"Failed to submit WebHook {webHook.Id} due to failure: {ex.Message}";
                Logger.LogError(message, ex);
            }
        }

        /// <summary>
        /// Creates an <see cref="HttpRequestMessage"/> containing the headers and body given a <paramref name="webHook"/>.
        /// </summary>
        /// <param name="webHook">A <see cref="WebHook"/> to be sent.</param>
        /// <param name="context"></param>
        /// <returns>A filled in <see cref="HttpRequestMessage"/>.</returns>
        protected virtual async Task<HttpRequestMessage> CreateWebHookRequestAsync(WebHook webHook, WebHookNotificationContext context)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            // Create WebHook request
            var request = new HttpRequestMessage(new HttpMethod(webHook.HttpMethod), webHook.Url);

            // Override the default payload if a liquid template has been provided
            var payload = context.DefaultPayload;
            if (!string.IsNullOrEmpty(webHook.PayloadTemplate))
            {
                payload = await _expressionEvaluator.RenderAsync(webHook, context);
            }

            // Fill in request body based on WebHook and payload
            request.Content = CreateWebHookRequestBody(webHook, payload);
            await SignWebHookRequestAsync(webHook, request);

            AddWebHookMetadata(webHook, context.EventName, request);

            AddCustomHeaders(webHook, request);

            return request;
        }

        /// <summary>
        /// Creates a <see cref="HttpContent"/> used as the <see cref="HttpRequestMessage"/> entity body for a webhook notification.
        /// </summary>
        /// <param name="webHook">A <see cref="WebHook"/> to be sent.</param>
        /// <param name="payload">The object representing the data to be sent with the webhook notification.</param>
        /// <returns>An initialized <see cref="HttpContent"/>.</returns>
        protected virtual HttpContent CreateWebHookRequestBody(WebHook webHook, JObject payload)
        {
            var body = payload ?? new JObject();
            
            if (webHook.ContentType == MediaTypeNames.FormUrlEncoded)
            {
                return new JsonFormUrlEncodedContent(body);
            }
            else
            {
                var serializedBody = body.ToString();
                return new StringContent(serializedBody, Encoding.UTF8, webHook.ContentType);
            }
        }

        /// <summary>
        /// Computes a SHA 256 signature of the request body and adds it to the <paramref name="request"/> as an
        /// HTTP header to the <see cref="HttpRequestMessage"/> along with the entity body.
        /// </summary>
        /// <param name="webHook">The current <see cref="WebHook"/>.</param>
        /// <param name="request">The request to add the signature to.</param>
        protected virtual async Task SignWebHookRequestAsync(WebHook webHook, HttpRequestMessage request)
        {
            if (webHook == null)
            {
                var message = $"Invalid \'{GetType().Name}\' instance: \'WebHook\' cannot be null.";
                throw new ArgumentException(message, nameof(webHook));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var secret = Encoding.UTF8.GetBytes(webHook.Secret);
            using (var hasher = new HMACSHA256(secret))
            {
                var body = await request.Content.ReadAsByteArrayAsync();
                var sha256 = hasher.ComputeHash(body);
                var headerValue = string.Format(CultureInfo.InvariantCulture, SignatureHeaderValueTemplate, ByteArrayToString(sha256));
                request.Headers.Add(SignatureHeaderName, headerValue);
            }
        }

        private void AddWebHookMetadata(WebHook webHook, string eventName, HttpRequestMessage request)
        {
            request.Headers.Add(HeaderIdName, webHook.Id);
            request.Headers.Add(HeaderEventName, eventName);
            request.Headers.Add(HeaderTenantName, _shellSettings.Name);
        }

        private void AddCustomHeaders(WebHook webHook, HttpRequestMessage request)
        {
            foreach (var kvp in webHook.Headers)
            {
                if (string.IsNullOrEmpty(kvp.Key)) continue;
                if (request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value)) continue;
                if (request.Content.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value)) continue;

                var message =
                    $"Could not add header field \'{kvp.Key}\' to the WebHook request for WebHook ID \'{webHook.Id}\'.";
                Logger.LogError(message);
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-","");
        }
    }
}