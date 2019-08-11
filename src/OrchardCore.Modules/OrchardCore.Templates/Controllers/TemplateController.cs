using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Controllers
{
    [Admin]
    public class TemplateController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly TemplatesManager _templatesManager;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        
        public TemplateController(
            IAuthorizationService authorizationService,
            TemplatesManager templatesManager,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IStringLocalizer<TemplateController> stringLocalizer,
            IHtmlLocalizer<TemplateController> htmlLocalizer,
            INotifier notifier)
        {
            _authorizationService = authorizationService;
            _templatesManager = templatesManager;
            New = shapeFactory;
            _siteService = siteService;
            _notifier = notifier;
            T = stringLocalizer;
            H = htmlLocalizer;
        }

        public dynamic New { get; set; }

        public IStringLocalizer T { get; set; }
        public IHtmlLocalizer H { get; set; }

        public async Task<IActionResult> Index(PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);
            var templatesDocument = await _templatesManager.GetTemplatesDocumentAsync();

            var count = templatesDocument.Templates.Count;

            var templates = templatesDocument.Templates.OrderBy(x => x.Key)
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count);

            var model = new TemplateIndexViewModel
            {
                Templates = templates.Select(x => new TemplateEntry { Name = x.Key, Template =x.Value }).ToList(),
                Pager = pagerShape
            };

            return View(model);
        }

        public async Task<IActionResult> Create(TemplateViewModel model, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
            {
                return Unauthorized();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new TemplateViewModel());
        }

        [HttpPost, ActionName("Create")]
        public async Task<IActionResult> CreatePost(TemplateViewModel model, string submit, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
            {
                return Unauthorized();
            }

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError(nameof(TemplateViewModel.Name), T["The name is mandatory."]);
                }
            }

            if (ModelState.IsValid)
            {
                var template = new Template { Content = model.Content, Description = model.Description };

                await _templatesManager.UpdateTemplateAsync(model.Name, template);
                if (submit != "SaveAndContinue")
                {
                    return RedirectToReturnUrlOrIndex(returnUrl);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string name, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
            {
                return Unauthorized();
            }

            var templatesDocument = await _templatesManager.GetTemplatesDocumentAsync();

            if (!templatesDocument.Templates.ContainsKey(name))
            {
                return RedirectToAction("Create", new { name, returnUrl });
            }

            var template = templatesDocument.Templates[name];

            var model = new TemplateViewModel
            {
                Name = name,
                Content = template.Content,
                Description = template.Description
            };

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string sourceName, TemplateViewModel model, string submit, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
            {
                return Unauthorized();
            }

            var templatesDocument = await _templatesManager.GetTemplatesDocumentAsync();

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError(nameof(TemplateViewModel.Name), T["The name is mandatory."]);
                }
            }

            if (!templatesDocument.Templates.ContainsKey(sourceName))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var template = new Template { Content = model.Content, Description = model.Description };


                await _templatesManager.RemoveTemplateAsync(sourceName);
                await _templatesManager.UpdateTemplateAsync(model.Name, template);
                if (submit != "SaveAndContinue")
                {
                    return RedirectToReturnUrlOrIndex(returnUrl);
                }                
            }

            // If we got this far, something failed, redisplay form
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string name, string returnUrl)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
            {
                return Unauthorized();
            }

            var templatesDocument = await _templatesManager.GetTemplatesDocumentAsync();

            if (!templatesDocument.Templates.ContainsKey(name))
            {
                return NotFound();
            }

            await _templatesManager.RemoveTemplateAsync(name);

            _notifier.Success(H["Template deleted successfully"]);
            
            return RedirectToReturnUrlOrIndex(returnUrl);
        }

        private IActionResult RedirectToReturnUrlOrIndex(string returnUrl)
        {
            if ((String.IsNullOrEmpty(returnUrl) == false) && (Url.IsLocalUrl(returnUrl)))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
