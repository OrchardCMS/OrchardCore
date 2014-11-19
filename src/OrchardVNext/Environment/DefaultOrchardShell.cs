using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using OrchardVNext.Environment.Configuration;
using System;
using System.Threading.Tasks;

namespace OrchardVNext.Environment {
    public interface IOrchardShell {
        void Activate();
        void Terminate();
    }

    public class DefaultOrchardShell : IOrchardShell {
        private readonly IRouteBuilder _routeBuilder;
        private readonly ShellSettings _shellSettings;

        public DefaultOrchardShell(IRouteBuilder routeBuilder,
            ShellSettings shellSettings) {
            _routeBuilder = routeBuilder;
            _shellSettings = shellSettings;
        }

        public void Activate() {
            var endpoint = new DelegateRouteEndpoint(async (context) =>
                                                        await context
                                                                .HttpContext
                                                                .Response
                                                                .WriteAsync("Hello, World! from " + _shellSettings.Name));

            _routeBuilder.AddPrefixRoute(_shellSettings.RequestUrlPrefix, "hello/world", endpoint);
        }

        public void Terminate() {
        }
    }

    public class DelegateRouteEndpoint : IRouter {
        public delegate Task RoutedDelegate(RouteContext context);

        private readonly RoutedDelegate _appFunc;

        public DelegateRouteEndpoint(RoutedDelegate appFunc) {
            _appFunc = appFunc;
        }

        public async Task RouteAsync(RouteContext context) {
            await _appFunc(context);
            context.IsHandled = true;
        }

        public string GetVirtualPath(VirtualPathContext context) {
            // We don't really care what the values look like.
            context.IsBound = true;
            return null;
        }
    }

    public static class RouteBuilderExtensions {


        public static IRouteBuilder AddPrefixRoute(this IRouteBuilder routeBuilder,
                                                   string urlHost,
                                                   string prefix,
                                                   IRouter handler) {
            routeBuilder.Routes.Add(new PrefixRoute(handler, urlHost, prefix));
            return routeBuilder;
        }
    }

    internal class PrefixRoute : IRouter {
        private readonly IRouter _target;
        private readonly string _urlHost;
        private readonly string _prefix;

        public PrefixRoute(IRouter target, string urlHost, string prefix) {
            _target = target;
            _urlHost = urlHost;

            if (prefix == null) {
                prefix = "/";
            }
            else if (prefix.Length > 0 && prefix[0] != '/') {
                // owin.RequestPath starts with a /
                prefix = "/" + prefix;
            }

            if (prefix.Length > 1 && prefix[prefix.Length - 1] == '/') {
                prefix = prefix.Substring(0, prefix.Length - 1);
            }

            _prefix = prefix;
        }

        public async Task RouteAsync(RouteContext context) {
            if (context.HttpContext.Request.Host.Value == _urlHost) {

                var requestPath = context.HttpContext.Request.Path.Value ?? string.Empty;
                if (requestPath.StartsWith(_prefix, StringComparison.OrdinalIgnoreCase)) {
                    if (requestPath.Length > _prefix.Length) {
                        var lastCharacter = requestPath[_prefix.Length];
                        if (lastCharacter != '/' && lastCharacter != '#' && lastCharacter != '?') {
                            return;
                        }
                    }

                    await _target.RouteAsync(context);
                }
            }
        }

        public string GetVirtualPath(VirtualPathContext context) {
            return null;
        }
    }
}