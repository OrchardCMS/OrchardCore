using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.WebHooks.Abstractions.Services;
using OrchardCore.WebHooks.ViewModels;

namespace OrchardCore.WebHooks.Controllers
{
    [Admin]
    public class WebHookController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IWebHookStore _store;
        private readonly IWebHookEventManager _eventManager;
        private readonly INotifier _notifier;

        public WebHookController(
            IWebHookStore store,
            IWebHookEventManager eventManager,
            IAuthorizationService authorizationService,
            INotifier notifier,
            IStringLocalizer<WebHookController> stringLocalizer,
            IHtmlLocalizer<WebHookController> htmlLocalizer,
            ILogger<WebHookController> logger
            )
        {
            _store = store;
            _eventManager = eventManager;
            _authorizationService = authorizationService;
            _notifier = notifier;

            S = stringLocalizer;
            H = htmlLocalizer;
            Logger = logger;
        }

        public IStringLocalizer S { get; set; }

        public IHtmlLocalizer H { get; set; }

        public ILogger<WebHookController> Logger { get; }

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWebHooks))
            {
                return Unauthorized();
            }

            var webHooksList = await _store.GetAllWebHooksAsync();

            var model = new WebHookIndexViewModel
            {
                WebHooksList = webHooksList
            };

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWebHooks))
            {
                return Unauthorized();
            }

            var events = await _eventManager.GetAllWebHookEventsAsync();
            var model = new EditWebHookViewModel
            {
                Events = events
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EditWebHookViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWebHooks))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                await ProcessWebHookAsync(model);
                await _store.CreateWebHookAsync(model.WebHook);

                await _notifier.SuccessAsync(H["Webhook created successfully"]);
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            model.Events = await _eventManager.GetAllWebHookEventsAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWebHooks))
            {
                return Unauthorized();
            }

            var webHook = await _store.GetWebHookAsync(id);

            if (webHook == null)
            {
                return NotFound();
            }

            var events = await _eventManager.GetAllWebHookEventsAsync();
            var model = new EditWebHookViewModel
            {
                Events = events,
                WebHook = webHook,
                CustomPayload = webHook.PayloadTemplate != null,
                SubscribeAllEvents = webHook.Events.Contains("*")
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditWebHookViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWebHooks))
            {
                return Unauthorized();
            }

            var webHook = await _store.GetWebHookAsync(model.WebHook.Id);

            if (webHook == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await ProcessWebHookAsync(model);
                await _store.TryUpdateWebHook(model.WebHook);

                await _notifier.SuccessAsync(H["Webhook updated successfully"]);

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            model.Events = await _eventManager.GetAllWebHookEventsAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWebHooks))
            {
                return Unauthorized();
            }

            var webHook = await _store.GetWebHookAsync(id);

            if (webHook == null)
            {
                return NotFound();
            }

            await _store.DeleteWebHookAsync(id);

            await _notifier.SuccessAsync(H["Webhook deleted successfully"]);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Ping(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWebHooks))
            {
                return Unauthorized();
            }

            var webHook = await _store.GetWebHookAsync(id);

            if (webHook == null)
            {
                return NotFound();
            }

            var ping = new Ping();
            var uri = new Uri(webHook.Url);

            PingReply result = null;
            string errorMessage = string.Empty;
            try
            {
                result = ping.Send(uri.Host);
            }
            catch (PingException ex)
            {
                errorMessage = ex.Message;
                Logger.LogInformation(ex, "Failed to ping {0} due to failure: {1}.", uri.Host, errorMessage);
            }

            if (result == null || result.Status != IPStatus.Success)
            {
                await _notifier.ErrorAsync(H["Failed to ping {0}. {1}", uri.Host, errorMessage]);
            }
            else
            {
                await _notifier.SuccessAsync(H["Successfully pinged {0}", uri.Host]);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task ProcessWebHookAsync(EditWebHookViewModel model)
        {
            // Clear all events so that the event provider can fallback to the wildcard event
            if (model.SubscribeAllEvents)
            {
                model.WebHook.Events.Clear();
            }

            // Clear the custom payload template if we aren't using it
            if (!model.CustomPayload)
            {
                model.WebHook.PayloadTemplate = null;
            }

            // Remove empty custom headers
            model.WebHook.Headers = model.WebHook.Headers.Where(header => !string.IsNullOrWhiteSpace(header.Key)).ToList();

            // Validate and optimize the webhook's event subscriptions
            model.WebHook.Events = await _eventManager.NormalizeEventsAsync(model.WebHook.Events);
        }
    }
}
