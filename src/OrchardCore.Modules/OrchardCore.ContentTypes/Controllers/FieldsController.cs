using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Utilities;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.ContentTypes.Services;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.Data.Documents;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Routing;

namespace OrchardCore.ContentTypes.Controllers;

[Admin]
[RequireFeatures("OrchardCore.ContentFields", "OrchardCore.ContentTypes")]
public class FieldsController : Controller
{
    private readonly IContentDefinitionService _contentDefinitionService;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDocumentStore _documentStore;
    private readonly IContentDefinitionDisplayManager _contentDefinitionDisplayManager;
    private readonly IHtmlLocalizer H;
    private readonly IStringLocalizer S;
    private readonly INotifier _notifier;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    public FieldsController(
        IContentDefinitionDisplayManager contentDefinitionDisplayManager,
        IContentDefinitionService contentDefinitionService,
        IContentDefinitionManager contentDefinitionManager,
        IAuthorizationService authorizationService,
        IDocumentStore documentStore,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IStringLocalizer<AdminController> stringLocalizer,
        INotifier notifier,
        IUpdateModelAccessor updateModelAccessor)
    {
        _notifier = notifier;
        _contentDefinitionDisplayManager = contentDefinitionDisplayManager;
        _documentStore = documentStore;
        _authorizationService = authorizationService;
        _contentDefinitionService = contentDefinitionService;
        _contentDefinitionManager = contentDefinitionManager;
        _updateModelAccessor = updateModelAccessor;

        H = htmlLocalizer;
        S = stringLocalizer;
    }

    public async Task<ActionResult> AddFieldTo(string id, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var partViewModel = _contentDefinitionService.LoadPart(id);

        if (partViewModel == null)
        {
            return NotFound();
        }

        var viewModel = new AddFieldViewModel
        {
            Part = partViewModel.PartDefinition,
            Fields = _contentDefinitionService.GetFields().Select(x => x.Name).OrderBy(x => x).ToList()
        };

        ViewData["ReturnUrl"] = returnUrl;
        return View(viewModel);
    }

    [HttpPost, ActionName("AddFieldTo")]
    public async Task<ActionResult> AddFieldToPOST(AddFieldViewModel viewModel, string id, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var partViewModel = _contentDefinitionService.LoadPart(id);

        if (partViewModel == null)
        {
            return NotFound();
        }

        var partDefinition = partViewModel.PartDefinition;

        viewModel.DisplayName = viewModel.DisplayName?.Trim() ?? String.Empty;
        viewModel.Name ??= String.Empty;

        if (String.IsNullOrWhiteSpace(viewModel.DisplayName))
        {
            ModelState.AddModelError("DisplayName", S["The Display Name can't be empty."]);
        }

        if (partDefinition.Fields.Any(f => String.Equals(f.DisplayName().Trim(), viewModel.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("DisplayName", S["A field with the same Display Name already exists."]);
        }

        if (String.IsNullOrWhiteSpace(viewModel.Name))
        {
            ModelState.AddModelError("Name", S["The Technical Name can't be empty."]);
        }

        if (partDefinition.Fields.Any(f => String.Equals(f.Name.Trim(), viewModel.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("Name", S["A field with the same Technical Name already exists."]);
        }

        if (!String.IsNullOrWhiteSpace(viewModel.Name) && !viewModel.Name[0].IsLetter())
        {
            ModelState.AddModelError("Name", S["The Technical Name must start with a letter."]);
        }

        if (!String.Equals(viewModel.Name, viewModel.Name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Name", S["The Technical Name contains invalid characters."]);
        }

        if (!ModelState.IsValid)
        {
            viewModel.Part = partDefinition;
            viewModel.Fields = _contentDefinitionService.GetFields().Select(x => x.Name).OrderBy(x => x).ToList();

            await _documentStore.CancelAsync();

            ViewData["ReturnUrl"] = returnUrl;
            return View(viewModel);
        }

        _contentDefinitionService.AddFieldToPart(viewModel.Name, viewModel.DisplayName, viewModel.FieldTypeName, partDefinition.Name);

        await _notifier.SuccessAsync(H["The field \"{0}\" has been added.", viewModel.DisplayName]);

        if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return this.Redirect(returnUrl, true);
        }

        return RedirectToAction(nameof(EditField), new { id, viewModel.Name });
    }

    public async Task<ActionResult> EditField(string id, string name, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var partViewModel = _contentDefinitionService.GetPart(id);

        if (partViewModel == null)
        {
            return NotFound();
        }

        var partFieldDefinition = partViewModel.PartDefinition.Fields.FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

        if (partFieldDefinition == null)
        {
            return NotFound();
        }

        var viewModel = new EditFieldViewModel
        {
            Name = partFieldDefinition.Name,
            Editor = partFieldDefinition.Editor(),
            DisplayMode = partFieldDefinition.DisplayMode(),
            DisplayName = partFieldDefinition.DisplayName(),
            PartFieldDefinition = partFieldDefinition,
            Shape = await _contentDefinitionDisplayManager.BuildPartFieldEditorAsync(partFieldDefinition, _updateModelAccessor.ModelUpdater)
        };

        ViewData["ReturnUrl"] = returnUrl;

        return View(viewModel);
    }

    [HttpPost, ActionName("EditField")]
    [FormValueRequired("submit.Save")]
    public async Task<ActionResult> EditFieldPOST(string id, EditFieldViewModel viewModel, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        if (viewModel == null)
        {
            return NotFound();
        }

        var partViewModel = _contentDefinitionService.LoadPart(id);

        if (partViewModel == null)
        {
            return NotFound();
        }

        var field = _contentDefinitionManager.LoadPartDefinition(id).Fields.FirstOrDefault(x => String.Equals(x.Name, viewModel.Name, StringComparison.OrdinalIgnoreCase));

        if (field == null)
        {
            return NotFound();
        }

        viewModel.PartFieldDefinition = field;

        if (field.DisplayName() != viewModel.DisplayName)
        {
            // prevent null reference exception in validation
            viewModel.DisplayName = viewModel.DisplayName?.Trim() ?? String.Empty;

            if (String.IsNullOrWhiteSpace(viewModel.DisplayName))
            {
                ModelState.AddModelError("DisplayName", S["The Display Name can't be empty."]);
            }

            if (_contentDefinitionService.LoadPart(partViewModel.Name).PartDefinition.Fields.Any(t => t.Name != viewModel.Name && String.Equals(t.DisplayName().Trim(), viewModel.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("DisplayName", S["A field with the same Display Name already exists."]);
            }

            if (!ModelState.IsValid)
            {
                // Calls update to build editor shape with the display name validation failures, and other validation errors.
                viewModel.Shape = await _contentDefinitionDisplayManager.UpdatePartFieldEditorAsync(field, _updateModelAccessor.ModelUpdater);
                await _documentStore.CancelAsync();

                ViewData["ReturnUrl"] = returnUrl;

                return View(viewModel);
            }

            await _notifier.InformationAsync(H["Display name changed to {0}.", viewModel.DisplayName]);
        }

        _contentDefinitionService.AlterField(partViewModel, viewModel);

        // Refresh the local field variable in case it has been altered
        field = _contentDefinitionManager.LoadPartDefinition(id).Fields.FirstOrDefault(x => String.Equals(x.Name, viewModel.Name, StringComparison.OrdinalIgnoreCase));

        viewModel.Shape = await _contentDefinitionDisplayManager.UpdatePartFieldEditorAsync(field, _updateModelAccessor.ModelUpdater);

        if (!ModelState.IsValid)
        {
            await _documentStore.CancelAsync();

            ViewData["ReturnUrl"] = returnUrl;

            return View(viewModel);
        }

        await _notifier.SuccessAsync(H["The \"{0}\" field settings have been saved.", field.DisplayName()]);

        if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return this.Redirect(returnUrl, true);
        }

        // Redirect to the type editor if a type exists with this name
        var typeViewModel = _contentDefinitionService.LoadType(id);
        if (typeViewModel != null)
        {
            return RedirectToAction(nameof(AdminController.Edit), typeof(AdminController).ControllerName(), new { id });
        }

        return RedirectToAction(nameof(AdminController.EditPart), typeof(AdminController).ControllerName(), new { id });
    }

    [HttpPost, ActionName("RemoveFieldFrom")]
    public async Task<ActionResult> RemoveFieldFromPOST(string id, string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var partViewModel = _contentDefinitionService.LoadPart(id);

        if (partViewModel == null)
        {
            return NotFound();
        }

        var field = partViewModel.PartDefinition.Fields.FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

        if (field == null)
        {
            return NotFound();
        }

        _contentDefinitionService.RemoveFieldFromPart(name, partViewModel.Name);

        await _notifier.SuccessAsync(H["The \"{0}\" field has been removed.", field.DisplayName()]);

        if (_contentDefinitionService.LoadType(id) != null)
        {
            return RedirectToAction(nameof(AdminController.Edit), typeof(AdminController).ControllerName(), new { id });
        }

        return RedirectToAction(nameof(AdminController.EditPart), typeof(AdminController).ControllerName(), new { id });
    }
}
