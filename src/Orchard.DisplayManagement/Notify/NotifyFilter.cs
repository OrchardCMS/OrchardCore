using System;
using System.Collections.Generic;
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
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly ShellSettings _shellSettings;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        private IList<NotifyEntry> _existingEntries;
        private bool _shouldDeleteCookie;
        private string _tenantPath;
        private readonly HtmlEncoder _htmlEncoder;

        public NotifyFilter(
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
            _notifier = notifier;
            _shapeFactory = shapeFactory;

            _tenantPath = "/" + _shellSettings.RequestUrlPrefix;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var messages = Convert.ToString(filterContext.HttpContext.Request.Cookies[CookiePrefix]);
            if (String.IsNullOrEmpty(messages))
            {
                return;
            }

            IList<NotifyEntry> messageEntries;

            messageEntries = DeserializeNotifyEntries(messages);
            if(messageEntries == null)
            {
                // An error occured during deserialization
                _shouldDeleteCookie = true;
                return;
            }

            if (!messageEntries.Any())
            {
                return;
            }

            // Make the notifications available for the rest of the current request.
            _existingEntries = messageEntries;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var messageEntries = _notifier.List();
            _existingEntries = _existingEntries ?? new List<NotifyEntry>();

            // Don't touch temp data if there's no work to perform.
            if (!messageEntries.Any() && !_existingEntries.Any())
            {
                return;
            }

            // Assign values to the Items collection instead of TempData and
            // combine any existing entries added by the previous request with new ones.

            _existingEntries = messageEntries.Concat(_existingEntries).ToList();

            // Result is not a view, so assume a redirect and assign values to TemData.
            // String data type used instead of complex array to be session-friendly.
            if (!(filterContext.Result is ViewResult) && _existingEntries.Any())
            {
                filterContext.HttpContext.Response.Cookies.Append(CookiePrefix, SerializeNotifyEntry(_existingEntries.ToArray()), new CookieOptions { HttpOnly = true, Path = _tenantPath });
            }
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if ((filterContext.Result is ViewResult) || _shouldDeleteCookie)
            {
                filterContext.HttpContext.Response.Cookies.Delete(CookiePrefix, new CookieOptions { Path = _tenantPath });
            }

            if (!(filterContext.Result is ViewResult))
            {
                return;
            }

            var messageEntries = _existingEntries ?? new List<NotifyEntry>();

            if (messageEntries.Count == 0)
            {
                return;
            }

            var messagesZone = _layoutAccessor.GetLayout().Zones["Messages"];
            foreach (var messageEntry in messageEntries)
            {
                messagesZone = messagesZone.Add(_shapeFactory.Message(messageEntry));
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
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

        private NotifyEntry[] DeserializeNotifyEntries(string value)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new NotifyEntryConverter(_htmlEncoder));

            try
            {
                var protector = CreateTenantProtector();
                var decoded = protector.Unprotect(WebUtility.UrlDecode(value));
                return JsonConvert.DeserializeObject<NotifyEntry[]>(decoded, settings);
            }
            catch
            {
                return null;
            }
        }
    }
}