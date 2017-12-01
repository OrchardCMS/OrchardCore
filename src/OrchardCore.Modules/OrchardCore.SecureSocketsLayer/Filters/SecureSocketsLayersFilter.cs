using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Entities;
using OrchardCore.SecureSocketsLayer.Models;
using OrchardCore.SecureSocketsLayer.Services;
using OrchardCore.Settings;

namespace OrchardCore.SecureSocketsLayer.Filters
{

    public class SecureSocketsLayersFilter : IAsyncAuthorizationFilter, IOrderedFilter
    {
        private readonly ISecureSocketsLayerService _sslService;

        public SecureSocketsLayersFilter(ISecureSocketsLayerService sslService)
        {
            _sslService = sslService;
        }

        /// <inheritdoc />
        public int Order { get; set; }

        /// <summary>
        /// Called early in the filter pipeline to confirm request is authorized. Confirms requests are received over
        /// HTTPS. Takes no action for HTTPS requests. Otherwise if it was a GET request, sets
        /// <see cref="AuthorizationFilterContext.Result"/> to a result which will redirect the client to the HTTPS
        /// version of the request URI. Otherwise, sets <see cref="AuthorizationFilterContext.Result"/> to a result
        /// which will set the status code to <c>403</c> (Forbidden).
        /// </summary>
        /// <inheritdoc />
        public async Task OnAuthorizationAsync(AuthorizationFilterContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            if (!filterContext.HttpContext.Request.IsHttps)
            {
                var sslSettings = await _sslService.GetSettingsAsync().ConfigureAwait(false);
                if (sslSettings.RequireHttps)
                    HandleNonHttpsRequest(filterContext, sslSettings);
            }
        }

        /// <summary>
        /// Called from <see cref="OnAuthorization"/> if the request is not received over HTTPS. Expectation is
        /// <see cref="AuthorizationFilterContext.Result"/> will not be <c>null</c> after this method returns.
        /// </summary>
        /// <param name="filterContext">The <see cref="AuthorizationFilterContext"/> to update.</param>
        /// <param name="sslSettings"></param>
        /// <remarks>
        /// If it was a GET request, default implementation sets <see cref="AuthorizationFilterContext.Result"/> to a
        /// result which will redirect the client to the HTTPS version of the request URI. Otherwise, default
        /// implementation sets <see cref="AuthorizationFilterContext.Result"/> to a result which will set the status
        /// code to <c>403</c> (Forbidden).
        /// </remarks>
        protected virtual void HandleNonHttpsRequest(AuthorizationFilterContext filterContext, SslSettings sslSettings)
        {

            // only redirect for GET requests, otherwise the browser might not propagate the verb and request
            // body correctly.
            if (!string.Equals(filterContext.HttpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
            {
                filterContext.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
            else
            {
                var request = filterContext.HttpContext.Request;

                var host = request.Host;
                if (sslSettings.SslPort.HasValue && sslSettings.SslPort > 0)
                {
                    // a specific SSL port is specified
                    host = new HostString(host.Host, sslSettings.SslPort.Value);
                }
                else
                {
                    // clear the port
                    host = new HostString(host.Host);
                }

                var newUrl = string.Concat(
                    "https://",
                    host.ToUriComponent(),
                    request.PathBase.ToUriComponent(),
                    request.Path.ToUriComponent(),
                    request.QueryString.ToUriComponent());

                // redirect to HTTPS version of page
                filterContext.Result = new RedirectResult(newUrl, sslSettings.RequireHttpsPermanent);
            }
        }
    }
}
//    public class SecureSocketsLayersFilter : IActionFilter {
//        private readonly ISecureSocketsLayerService _sslService;

//        public SecureSocketsLayersFilter(ISecureSocketsLayerService sslService)
//        {
//            _sslService = sslService;
//        }


//        public void OnActionExecuted(ActionExecutedContext filterContext) {
//        }

//        public void OnActionExecuting(ActionExecutingContext filterContext) {
//            var settings = _sslService.GetSettingsAsync();

//            if (filterContext.IsChildAction || !settings.RequireHttps) {
//                return;
//            }

//            var user = filterContext.HttpContext.User;
//            var secure =
//                (user != null && user.Identity.IsAuthenticated) ||
//                _sslService.ShouldBeSecure(filterContext);

//            var request = filterContext.HttpContext.Request;

//            // redirect to a secured connection ?
//            if (secure && !request.IsSecureConnection) {
//                var secureActionUrl = AppendQueryString(
//                    request.QueryString,
//                    _sslService.SecureActionUrl(
//                        filterContext.ActionDescriptor.ActionName,
//                        filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
//                        filterContext.RequestContext.RouteData.Values));

//                filterContext.Result = new RedirectResult(secureActionUrl);
//                return;
//            }

//            // non auth page on a secure canal
//            // nb: needed as the ReturnUrl for LogOn doesn't force the scheme to http, and reuses the current one
//            // Also don't force http on ajax requests.
//            if (!secure && request.IsSecureConnection && !request.IsAjaxRequest()) {
//                var insecureActionUrl = AppendQueryString(
//                    request.QueryString,
//                    _sslService.InsecureActionUrl(
//                        filterContext.ActionDescriptor.ActionName,
//                        filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
//                        filterContext.RequestContext.RouteData.Values));

//                filterContext.Result = new RedirectResult(insecureActionUrl);
//            }
//        }

//        private static string AppendQueryString(NameValueCollection queryString, string url) {
//            if (queryString.Count > 0) {
//                url += '?' + queryString.ToString();
//            }
//            return url;
//        }
//    }
//}