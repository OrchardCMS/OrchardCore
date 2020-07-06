using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Placements.Services;
using OrchardCore.Placements.ViewModels;

namespace OrchardCore.Placements.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly IPlacementRulesService _placementRulesService;
        private readonly IHtmlLocalizer H;
        private readonly IStringLocalizer S;
        private readonly INotifier _notifier;

        public AdminController(
            ILogger<AdminController> logger,
            IAuthorizationService authorizationService,
            IPlacementRulesService placementRulesService,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            IStringLocalizer<AdminController> stringLocalizer,
            INotifier notifier)
        {
            _logger = logger;
            _authorizationService = authorizationService;
            _placementRulesService = placementRulesService;
            _notifier = notifier;

            H = htmlLocalizer;
            S = stringLocalizer;
        }

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePlacements))
            {
                return Forbid();
            }

            var shapeTypes = _placementRulesService.ListShapePlacements();

            return View(new ListShapePlacementsViewModel
            {
                ShapePlacements = shapeTypes.Select(entry => new ShapePlacementViewModel
                {
                    ShapeType = entry.Key
                })
            });
        }

        public async Task<IActionResult> Create(string suggestion, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePlacements))
            {
                return Forbid();
            }

            var template = new PlacementNode[] { new PlacementNode() };

            var viewModel = new EditShapePlacementViewModel
            {
                Creating = true,
                ShapeType = suggestion,
                Nodes = JsonConvert.SerializeObject(template, Formatting.Indented)
            };

            ViewData["ReturnUrl"] = returnUrl;
            return View("Edit", viewModel);
        }

        public async Task<IActionResult> Edit(string id, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePlacements))
            {
                return Forbid();
            }

            var placementNodes = _placementRulesService.GetShapePlacements(id);

            var viewModel = new EditShapePlacementViewModel
            {
                ShapeType = id,
                Nodes = JsonConvert.SerializeObject(placementNodes, Formatting.Indented)
            };

            ViewData["ReturnUrl"] = returnUrl;
            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        public async Task<IActionResult> Edit(EditShapePlacementViewModel viewModel, string submit, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePlacements))
            {
                return Forbid();
            }

            ViewData["ReturnUrl"] = returnUrl;

            try
            {
                var placementNodes = JsonConvert.DeserializeObject<PlacementNode[]>(viewModel.Nodes);

                await _placementRulesService.UpdateShapePlacementsAsync(viewModel.ShapeType, placementNodes);

                _notifier.Success(H["The \"{0}\" placement have been saved.", viewModel.ShapeType]);
            }
            catch(JsonReaderException jsonException)
            {
                _notifier.Error(H["An error occurred while parsing the placement<br/>{0}", jsonException.Message]);
                return View(viewModel);
            }
            catch (Exception e)
            {
                _notifier.Error(H["An error occurred while saving the placement"]);
                _logger.LogError(e, "An error occurred while saving the placement");
                return View(viewModel);
            }

            if (submit != "SaveAndContinue")
            {
                return RedirectToReturnUrlOrIndex(returnUrl);
            }

            return View(viewModel);
        }

        [HttpPost, ActionName("Remove")]
        public async Task<IActionResult> Remove(string id, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePlacements))
            {
                return Forbid();
            }

            await _placementRulesService.RemoveShapePlacementsAsync(id);
            _notifier.Success(H["The \"{0}\" placement has been removed.", id]);

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
