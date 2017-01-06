using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Nancy.Owin;
using Nancy.Helpers;
using Nancy.IO;

namespace Microsoft.AspNetCore.Nancy.Modules
{
    using global::Nancy;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>,
       System.Threading.Tasks.Task>;

    using MidFunc = System.Func<System.Func<System.Collections.Generic.IDictionary<string, object>,
            System.Threading.Tasks.Task>, System.Func<System.Collections.Generic.IDictionary<string, object>,
            System.Threading.Tasks.Task>>;

    /// <summary>
    /// OWIN extensions for the delegate-based approach.
    /// </summary>
    public static class DelegateExtensions
    {
        /// <summary>
        /// Adds Nancy to the OWIN pipeline.
        /// </summary>
        /// <param name="builder">The application builder delegate.</param>
        /// <param name="action">A configuration builder action.</param>
        /// <returns>The application builder delegate.</returns>
        public static Action<MidFunc> UseANancy(this Action<MidFunc> builder, Action<NancyOptions> action)
        {
            var options = new NancyOptions();

            action(options);

            return builder.UseANancy(options);
        }

        /// <summary>
        /// Adds Nancy to the OWIN pipeline.
        /// </summary>
        /// <param name="builder">The application builder delegate.</param>
        /// <param name="options">The Nancy options.</param>
        /// <returns>The application builder delegate.</returns>
        public static Action<MidFunc> UseANancy(this Action<MidFunc> builder, NancyOptions options = null)
        {
            var nancyOptions = options ?? new NancyOptions();

            builder(InternalNancyMiddleware.UseANancy(nancyOptions).Invoke);

            return builder;
        }
    }

    /// <summary>
    /// Nancy middleware for OWIN.
    /// </summary>
    public static class InternalNancyMiddleware
    {
        /// <summary>
        /// The request environment key
        /// </summary>
        public const string RequestEnvironmentKey = "OWIN_REQUEST_ENVIRONMENT";

        /// <summary>
        /// Use Nancy in an OWIN pipeline
        /// </summary>
        /// <param name="configuration">A delegate to configure the <see cref="NancyOptions"/>.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc UseANancy(Action<NancyOptions> configuration)
        {
            var options = new NancyOptions();
            configuration(options);
            return UseANancy(options);
        }

        /// <summary>
        /// Use Nancy in an OWIN pipeline
        /// </summary>
        /// <param name="options">An <see cref="NancyOptions"/> to configure the Nancy middleware</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc UseANancy(NancyOptions options = null)
        {
            options = options ?? new NancyOptions();
            options.Bootstrapper.Initialise();
            var engine = options.Bootstrapper.GetEngine();

            return
                next =>
                    async environment =>
                    {
                        var owinRequestMethod = Get<string>(environment, "owin.RequestMethod");
                        var owinRequestScheme = Get<string>(environment, "owin.RequestScheme");
                        var owinRequestHeaders = Get<IDictionary<string, string[]>>(environment, "owin.RequestHeaders");
                        var owinRequestPathBase = Get<string>(environment, "owin.RequestPathBase");
                        var owinRequestPath = Get<string>(environment, "owin.RequestPath");
                        var owinRequestQueryString = Get<string>(environment, "owin.RequestQueryString");
                        var owinRequestBody = Get<Stream>(environment, "owin.RequestBody");
                        var owinRequestProtocol = Get<string>(environment, "owin.RequestProtocol");
                        var owinCallCancelled = Get<CancellationToken>(environment, "owin.CallCancelled");
                        var owinRequestHost = GetHeader(owinRequestHeaders, "Host") ?? Dns.GetHostName();
                        var owinUser = GetUser(environment);

                        X509Certificate2 certificate = null;
                        if (options.EnableClientCertificates)
                        {
                            var clientCertificate = new X509Certificate2(Get<X509Certificate>(environment, "ssl.ClientCertificate").Export(X509ContentType.Cert));
                            certificate = clientCertificate ?? null;
                        }

                        var serverClientIp = Get<string>(environment, "server.RemoteIpAddress");

                        var url = CreateUrl(owinRequestHost, owinRequestScheme, owinRequestPathBase, owinRequestPath, owinRequestQueryString);

                        var nancyRequestStream = new RequestStream(owinRequestBody, ExpectedLength(owinRequestHeaders), StaticConfiguration.DisableRequestStreamSwitching ?? false);

                        var nancyRequest = new Request(
                                owinRequestMethod,
                                url,
                                nancyRequestStream,
                                owinRequestHeaders.ToDictionary(kv => kv.Key, kv => (IEnumerable<string>)kv.Value, StringComparer.OrdinalIgnoreCase),
                                serverClientIp,
                                certificate,
                                owinRequestProtocol);

                        var nancyContext = await engine.HandleRequest(
                            nancyRequest,
                            StoreEnvironment(environment, owinUser),
                            owinCallCancelled).ConfigureAwait(false);

                        await RequestComplete(nancyContext, environment, options.PerformPassThrough, next).ConfigureAwait(false);
                    };
        }

        /// <summary>
        /// Gets a delegate to handle converting a nancy response
        /// to the format required by OWIN and signals that the we are
        /// now complete.
        /// </summary>
        /// <param name="context">The Nancy Context.</param>
        /// <param name="environment">OWIN environment.</param>
        /// <param name="next">The next stage in the OWIN pipeline.</param>
        /// <param name="performPassThrough">A predicate that will allow the caller to determine if the request passes through to the 
        /// next stage in the owin pipeline.</param>
        /// <returns>Delegate</returns>
        private static Task RequestComplete(
            NancyContext context,
            IDictionary<string, object> environment,
            Func<NancyContext, bool> performPassThrough,
            AppFunc next)
        {
            var owinResponseHeaders = Get<IDictionary<string, string[]>>(environment, "owin.ResponseHeaders");
            var owinResponseBody = Get<Stream>(environment, "owin.ResponseBody");

            var nancyResponse = context.Response;
            if (!performPassThrough(context))
            {
                environment["owin.ResponseStatusCode"] = (int)nancyResponse.StatusCode;

                if (nancyResponse.ReasonPhrase != null)
                {
                    environment["owin.ResponseReasonPhrase"] = nancyResponse.ReasonPhrase;
                }

                foreach (var responseHeader in nancyResponse.Headers)
                {
                    owinResponseHeaders[responseHeader.Key] = new[] { responseHeader.Value };
                }

                if (!string.IsNullOrWhiteSpace(nancyResponse.ContentType))
                {
                    owinResponseHeaders["Content-Type"] = new[] { nancyResponse.ContentType };
                }

                if (nancyResponse.Cookies != null && nancyResponse.Cookies.Count != 0)
                {
                    const string setCookieHeaderKey = "Set-Cookie";
                    string[] setCookieHeader = owinResponseHeaders.ContainsKey(setCookieHeaderKey)
                                                    ? owinResponseHeaders[setCookieHeaderKey]
                                                    : ArrayCache.Empty<string>();
                    owinResponseHeaders[setCookieHeaderKey] = setCookieHeader
                        .Concat(nancyResponse.Cookies.Select(cookie => cookie.ToString()))
                        .ToArray();
                }

                nancyResponse.Contents(owinResponseBody);
            }
            else
            {
                return next(environment);
            }

            context.Dispose();

            return TaskHelpers.CompletedTask;
        }

        private static T Get<T>(IDictionary<string, object> env, string key)
        {
            object value;
            return env.TryGetValue(key, out value) && value is T ? (T)value : default(T);
        }

        private static string GetHeader(IDictionary<string, string[]> headers, string key)
        {
            string[] value;
            return headers.TryGetValue(key, out value) && value != null ? string.Join(",", value.ToArray()) : null;
        }

        private static ClaimsPrincipal GetUser(IDictionary<string, object> environment)
        {
            // OWIN 1.1
            object user;
            if (environment.TryGetValue("owin.RequestUser", out user))
            {
                return user as ClaimsPrincipal;
            }

            // check for Katana User
            if (environment.TryGetValue("server.User", out user))
            {
                return user as ClaimsPrincipal;
            }
            return null;
        }

        private static long ExpectedLength(IDictionary<string, string[]> headers)
        {
            var header = GetHeader(headers, "Content-Length");
            if (string.IsNullOrWhiteSpace(header))
                return 0;

            int contentLength;
            return int.TryParse(header, NumberStyles.Any, CultureInfo.InvariantCulture, out contentLength) ? contentLength : 0;
        }

        /// <summary>
        /// Creates the Nancy URL
        /// </summary>
        /// <param name="owinRequestHost">OWIN Hostname</param>
        /// <param name="owinRequestScheme">OWIN Scheme</param>
        /// <param name="owinRequestPathBase">OWIN Base path</param>
        /// <param name="owinRequestPath">OWIN Path</param>
        /// <param name="owinRequestQueryString">OWIN Querystring</param>
        /// <returns></returns>
        private static Url CreateUrl(
            string owinRequestHost,
            string owinRequestScheme,
            string owinRequestPathBase,
            string owinRequestPath,
            string owinRequestQueryString)
        {
            int? port = null;

            var hostnameParts = owinRequestHost.Split(':');
            if (hostnameParts.Length == 2)
            {
                owinRequestHost = hostnameParts[0];

                int tempPort;
                if (int.TryParse(hostnameParts[1], out tempPort))
                {
                    port = tempPort;
                }
            }

            var url = new Url
            {
                Scheme = owinRequestScheme,
                HostName = owinRequestHost,
                Port = port,
                BasePath = owinRequestPathBase,
                Path = owinRequestPath,
                Query = owinRequestQueryString,
            };
            return url;
        }

        /// <summary>
        /// Gets a delegate to store the OWIN environment and flow the user into the NancyContext
        /// </summary>
        /// <param name="environment">The OWIN environment.</param>
        /// <param name="user">The user as a ClaimsPrincipal.</param>
        /// <returns>Delegate</returns>
        private static Func<NancyContext, NancyContext> StoreEnvironment(IDictionary<string, object> environment, ClaimsPrincipal user)
        {
            return context =>
            {
                context.CurrentUser = user;
                environment["nancy.NancyContext"] = context;
                context.Items[RequestEnvironmentKey] = environment;
                return context;
            };
        }
    }
}
