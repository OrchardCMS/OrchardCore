using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Shortcodes.Models;
using OrchardCore.Shortcodes.Services;
using OrchardCore.Shortcodes.ViewModels;
using Parlot;

namespace OrchardCore.Shortcodes.Controllers
{
    [Feature("OrchardCore.Shortcodes.Templates")]
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ShortcodeTemplatesManager _shortcodeTemplatesManager;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly PagerOptions _pagerOptions;
        private readonly INotifier _notifier;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly dynamic New;

        public AdminController(
            IAuthorizationService authorizationService,
            ShortcodeTemplatesManager shortcodeTemplatesManager,
            ILiquidTemplateManager liquidTemplateManager,
            IOptions<PagerOptions> pagerOptions,
            INotifier notifier,
            IShapeFactory shapeFactory,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            IHtmlSanitizerService htmlSanitizerService
            )
        {
            _authorizationService = authorizationService;
            _shortcodeTemplatesManager = shortcodeTemplatesManager;
            _liquidTemplateManager = liquidTemplateManager;
            _pagerOptions = pagerOptions.Value;
            _notifier = notifier;
            New = shapeFactory;
            S = stringLocalizer;
            H = htmlLocalizer;
            _htmlSanitizerService = htmlSanitizerService;
        }

        public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageShortcodeTemplates))
            {
                return Forbid();
            }

            var pager = new Pager(pagerParameters, _pagerOptions.PageSize);
            var shortcodeTemplatesDocument = await _shortcodeTemplatesManager.GetShortcodeTemplatesDocumentAsync();

            var shortcodeTemplates = shortcodeTemplatesDocument.ShortcodeTemplates.ToList();

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                shortcodeTemplates = shortcodeTemplates.Where(x => x.Key.Contains(options.Search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var count = shortcodeTemplates.Count;

            shortcodeTemplates = shortcodeTemplates.OrderBy(x => x.Key)
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize).ToList();

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count);

            var model = new ShortcodeTemplateIndexViewModel
            {
                ShortcodeTemplates = shortcodeTemplates.Select(x => new ShortcodeTemplateEntry { Name = x.Key, ShortcodeTemplate = x.Value }).ToList(),
                Options = options,
                Pager = pagerShape
            };

            model.Options.ContentsBulkAction = new List<SelectListItem>() {
                new SelectListItem() { Text = S["Delete"], Value = nameof(ContentsBulkAction.Remove) }
            };

            return View("Index", model);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.Filter")]
        public ActionResult IndexFilterPOST(ShortcodeTemplateIndexViewModel model)
        {
            return RedirectToAction(nameof(Index), new RouteValueDictionary {
                { "Options.Search", model.Options.Search }
            });
        }

        public async Task<IActionResult> Create(ShortcodeTemplateViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageShortcodeTemplates))
            {
                return Forbid();
            }

            return View(new ShortcodeTemplateViewModel());
        }

        [HttpPost, ActionName("Create")]
        public async Task<IActionResult> CreatePost(ShortcodeTemplateViewModel model, string submit)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageShortcodeTemplates))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError(nameof(ShortcodeTemplateViewModel.Name), S["The name is mandatory."]);
                }
                else if (!IsValidShortcodeName(model.Name))
                {
                    ModelState.AddModelError(nameof(ShortcodeTemplateViewModel.Name), S["The name contains invalid characters."]);
                }
                else
                {
                    var shortcodeTemplatesDocument = await _shortcodeTemplatesManager.GetShortcodeTemplatesDocumentAsync();

                    if (shortcodeTemplatesDocument.ShortcodeTemplates.ContainsKey(model.Name))
                    {
                        ModelState.AddModelError(nameof(ShortcodeTemplateViewModel.Name), S["A template with the same name already exists."]);
                    }
                }

                if (String.IsNullOrEmpty(model.Content))
                {
                    ModelState.AddModelError(nameof(ShortcodeTemplateViewModel.Content), S["The template content is mandatory."]);
                }
                else if (!_liquidTemplateManager.Validate(model.Content, out var errors))
                {
                    ModelState.AddModelError(nameof(ShortcodeTemplateViewModel.Content), S["The template doesn't contain a valid Liquid expression. Details: {0}", String.Join(" ", errors)]);
                }
            }

            if (ModelState.IsValid)
            {
                var template = new ShortcodeTemplate
                {
                    Content = model.Content,
                    Hint = model.Hint,
                    Usage = _htmlSanitizerService.Sanitize(model.Usage),
                    DefaultValue = model.DefaultValue,
                    Categories = JsonConvert.DeserializeObject<string[]>(model.SelectedCategories)
                };

                await _shortcodeTemplatesManager.UpdateShortcodeTemplateAsync(model.Name, template);

                if (submit == "SaveAndContinue")
                {
                    return RedirectToAction(nameof(Edit), new { name = model.Name });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageShortcodeTemplates))
            {
                return Forbid();
            }

            var shortcodeTemplatesDocument = await _shortcodeTemplatesManager.GetShortcodeTemplatesDocumentAsync();

            if (!shortcodeTemplatesDocument.ShortcodeTemplates.ContainsKey(name))
            {
                return RedirectToAction(nameof(Create), new { name });
            }

            var template = shortcodeTemplatesDocument.ShortcodeTemplates[name];

            var model = new ShortcodeTemplateViewModel
            {
                Name = name,
                Content = template.Content,
                Hint = template.Hint,
                Usage = template.Usage,
                DefaultValue = template.DefaultValue,
                Categories = template.Categories
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string sourceName, ShortcodeTemplateViewModel model, string submit)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageShortcodeTemplates))
            {
                return Forbid();
            }

            var shortcodeTemplatesDocument = await _shortcodeTemplatesManager.LoadShortcodeTemplatesDocumentAsync();

            if (!shortcodeTemplatesDocument.ShortcodeTemplates.ContainsKey(sourceName))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError(nameof(ShortcodeTemplateViewModel.Name), S["The name is mandatory."]);
                }
                else if (!IsValidShortcodeName(model.Name))
                {
                    ModelState.AddModelError(nameof(ShortcodeTemplateViewModel.Name), S["The name contains invalid characters."]);
                }
                else if (!String.Equals(model.Name, sourceName, StringComparison.OrdinalIgnoreCase)
                    && shortcodeTemplatesDocument.ShortcodeTemplates.ContainsKey(model.Name))
                {
                    ModelState.AddModelError(nameof(ShortcodeTemplateViewModel.Name), S["A template with the same name already exists."]);
                }

                if (String.IsNullOrEmpty(model.Content))
                {
                    ModelState.AddModelError(nameof(ShortcodeTemplateViewModel.Content), S["The template content is mandatory."]);
                }
                else if (!_liquidTemplateManager.Validate(model.Content, out var errors))
                {
                    ModelState.AddModelError(nameof(ShortcodeTemplateViewModel.Content), S["The template doesn't contain a valid Liquid expression. Details: {0}", String.Join(" ", errors)]);
                }
            }

            if (ModelState.IsValid)
            {
                var template = new ShortcodeTemplate
                {
                    Content = model.Content,
                    Hint = model.Hint,
                    Usage = _htmlSanitizerService.Sanitize(model.Usage),
                    DefaultValue = model.DefaultValue,
                    Categories = JsonConvert.DeserializeObject<string[]>(model.SelectedCategories)
                };

                await _shortcodeTemplatesManager.RemoveShortcodeTemplateAsync(sourceName);

                await _shortcodeTemplatesManager.UpdateShortcodeTemplateAsync(model.Name, template);

                if (submit == "SaveAndContinue")
                {
                    return RedirectToAction(nameof(Edit), new { name = model.Name });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageShortcodeTemplates))
            {
                return Forbid();
            }

            var shortcodeTemplatesDocument = await _shortcodeTemplatesManager.LoadShortcodeTemplatesDocumentAsync();

            if (!shortcodeTemplatesDocument.ShortcodeTemplates.ContainsKey(name))
            {
                return NotFound();
            }

            await _shortcodeTemplatesManager.RemoveShortcodeTemplateAsync(name);

            await _notifier.SuccessAsync(H["Shortcode template deleted successfully."]);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.BulkAction")]
        public async Task<ActionResult> IndexPost(ViewModels.ContentOptions options, IEnumerable<string> itemIds)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageShortcodeTemplates))
            {
                return Forbid();
            }

            if (itemIds?.Count() > 0)
            {
                var shortcodeTemplatesDocument = await _shortcodeTemplatesManager.LoadShortcodeTemplatesDocumentAsync();
                var checkedContentItems = shortcodeTemplatesDocument.ShortcodeTemplates.Where(x => itemIds.Contains(x.Key));
                switch (options.BulkAction)
                {
                    case ContentsBulkAction.None:
                        break;
                    case ContentsBulkAction.Remove:
                        foreach (var item in checkedContentItems)
                        {
                            await _shortcodeTemplatesManager.RemoveShortcodeTemplateAsync(item.Key);
                        }
                        await _notifier.SuccessAsync(H["Shortcode templates successfully removed."]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private static bool IsValidShortcodeName(string name)
        {
            var scanner = new Scanner(name);
            return scanner.ReadIdentifier(out var result) && name.Length == result.Length;
        }
    }
}
