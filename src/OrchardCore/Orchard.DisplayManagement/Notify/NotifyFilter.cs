using System;
using System.Linq;
using System.Net;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Orchard.DisplayManagement.Layout;
using Orchard.Environment.Shell;

namespace Orchard.DisplayManagement.Notify
{
    public class NotifyFilter : IActionFilter, IResultFilter
    {
        public const string CookiePrefix = "orch_notify";
        private readonly INotifier _notifier;
        private readonly dynamic _shapeFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly ShellSettings _shellSettings;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        private NotifyEntry[] _existingEntries = Array.Empty<NotifyEntry>();
        private bool _shouldDeleteCookie;
        private string _tenantPath;
        private readonly HtmlEncoder _htmlEncoder;

        public NotifyFilter(
            IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            ILayoutAccessor layoutAccessor,
            IShapeFactory shapeFactory,
            ShellSettings shellSettings,
            IDataProtectionProvider dataProtectionProvider,
            HtmlEncoder htmlEncoder)
        {
            _htmlEncoder = htmlEncoder;
            _dataProtectionProvider = dataProtectionProvider;
            _shellSettings = shellSettings;

            _layoutAccessor = layoutAccessor;
            _httpContextAccessor = httpContextAccessor;
            _notifier = notifier;
            _shapeFactory = shapeFactory;

            _tenantPath = "/" + _shellSettings.RequestUrlPrefix;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var messages = Convert.ToString(_httpContextAccessor.HttpContext.Request.Cookies[CookiePrefix]);
            if (String.IsNullOrEmpty(messages))
            {
                return;
            }

            DeserializeNotifyEntries(messages, out NotifyEntry[] messageEntries);

            if (messageEntries == null)
            {
                // An error occured during deserialization
                _shouldDeleteCookie = true;
                return;
            }

            if (messageEntries.Length == 0)
            {
                return;
            }

            // Make the notifications available for the rest of the current request.
            _existingEntries = messageEntries;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var messageEntries = _notifier.List().ToArray();

            // Don't touch temp data if there's no work to perform.
            if (messageEntries.Length == 0 && _existingEntries.Length == 0)
            {
                return;
            }

            // Assign values to the Items collection instead of TempData and
            // combine any existing entries added by the previous request with new ones.

            _existingEntries = messageEntries.Concat(_existingEntries).ToArray();

            // Result is not a view, so assume a redirect and assign values to TemData.
            // String data type used instead of complex array to be session-friendly.
            if (!(filterContext.Result is ViewResult) && _existingEntries.Length > 0)
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Append(CookiePrefix, SerializeNotifyEntry(_existingEntries), new CookieOptions { HttpOnly = true, Path = _tenantPath });
            }
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (_shouldDeleteCookie)
            {
                DeleteCookies();
                return;
            }

            if (!(filterContext.Result is ViewResult))
            {
                return;
            }

            if (_existingEntries.Length == 0)
            {
                return;
            }

            var messagesZone = _layoutAccessor.GetLayout().Zones["Messages"];
            foreach (var messageEntry in _existingEntries)
            {
                messagesZone = messagesZone.Add(_shapeFactory.Message(messageEntry));
            }

            DeleteCookies();
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }

        private void DeleteCookies()
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(CookiePrefix, new CookieOptions { Path = _tenantPath });
        }

        private IDataProtector CreateTenantProtector()
        {
            return _dataProtectionProvider.CreateProtector(nameof(NotifyFilter), _tenantPath);
        }

        private string SerializeNotifyEntry(NotifyEntry[] notifyEntries)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new NotifyEntryConverter(_htmlEncoder));

            try
            {
                var protector = CreateTenantProtector();
                var signed = protector.Protect(JsonConvert.SerializeObject(notifyEntries, settings));
                return WebUtility.UrlEncode(signed);
            }
            catch
            {
                return null;
            }
        }

        private void DeserializeNotifyEntries(string value, out NotifyEntry[] messageEntries)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new NotifyEntryConverter(_htmlEncoder));

            try
            {
                var protector = CreateTenantProtector();
                var decoded = protector.Unprotect(WebUtility.UrlDecode(value));
                messageEntries = JsonConvert.DeserializeObject<NotifyEntry[]>(decoded, settings);
            }
            catch
            {
                messageEntries = null;
            }
        }
    }
}