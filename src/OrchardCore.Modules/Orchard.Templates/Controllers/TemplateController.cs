using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Orchard.Admin;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Notify;
using Orchard.Navigation;
using Orchard.Settings;
using Orchard.Templates.Models;
using Orchard.Templates.Services;
using Orchard.Templates.ViewModels;
using YesSql;

namespace Orchard.Templates.Controllers
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

            var pagerShape = New.Pager(pager).TotalItemCount(count);

            var model = new TemplateIndexViewModel
            {
                Templates = templates.Select(x => new TemplateEntry { Name = x.Key, Template = x.Value }).ToList(),
                Pager = pagerShape
            };

            return View(model);
        }

        public async Task<IActionResult> Create(TemplateViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
            {
                return Unauthorized();
            }

            return View(new TemplateViewModel());
        }

        [HttpPost, ActionName("Create")]
        public async Task<IActionResult> CreatePost(TemplateViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.View))
                {
                    ModelState.AddModelError(nameof(TemplateViewModel.Extension), T["The view is mandatory."]);
                }

                if (String.IsNullOrWhiteSpace(model.Extension))
                {
                    ModelState.AddModelError(nameof(TemplateViewModel.Extension), T["The extension is mandatory."]);
                }

                if (String.IsNullOrWhiteSpace(model.Theme))
                {
                    ModelState.AddModelError(nameof(TemplateViewModel.Theme), T["The theme is mandatory."]);
                }
            }

            if (ModelState.IsValid)
            {
                var template = new Template
                {
                    View = model.View,
                    Extension = model.Extension,
                    Theme = model.Theme,
                    Content = model.Content,
                    Description = model.Description
                };

                await _templatesManager.UpdateTemplateAsync(string.Format("{0}/Views/{1}{2}",
                    model.Theme, model.View, model.Extension), template);

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string name)
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

            var template = templatesDocument.Templates[name];

            var model = new TemplateViewModel
            {
                View = template.View,
                Extension = template.Extension,
                Theme = template.Theme,
                Content = template.Content,
                Description = template.Description
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TemplateViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
            {
                return Unauthorized();
            }

            var templatesDocument = await _templatesManager.GetTemplatesDocumentAsync();

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.View))
                {
                    ModelState.AddModelError(nameof(TemplateViewModel.View), T["The view is mandatory."]);
                }
            }

            var path = string.Format("{0}/Views/{1}{2}", model.Theme, model.View, model.Extension);

            if (!templatesDocument.Templates.ContainsKey(path))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var template = new Template
                {
                    View = model.View,
                    Extension = model.Extension,
                    Theme = model.Theme,
                    Content = model.Content,
                    Description = model.Description
                };

                await _templatesManager.UpdateTemplateAsync(path, template);

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string name)
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
            
            return RedirectToAction(nameof(Index));
        }
    }
}
