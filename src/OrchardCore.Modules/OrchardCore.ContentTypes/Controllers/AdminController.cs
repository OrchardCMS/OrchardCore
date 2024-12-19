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
using OrchardCore.Routing;

namespace OrchardCore.ContentTypes.Controllers;

public sealed class AdminController : Controller
{
    private readonly IContentDefinitionService _contentDefinitionService;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDocumentStore _documentStore;
    private readonly IContentDefinitionDisplayManager _contentDefinitionDisplayManager;
    private readonly INotifier _notifier;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public AdminController(
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

    public Task<ActionResult> Index()
    {
        return List();
    }

    #region Types

    [Admin("ContentTypes/List", "ListContentTypes")]
    public async Task<ActionResult> List()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewContentTypes))
        {
            return Forbid();
        }

        return View("List", new ListContentTypesViewModel
        {
            Types = await _contentDefinitionService.GetTypesAsync()
        });
    }

    [Admin("ContentTypes/Create", "CreateType")]
    public async Task<ActionResult> Create(string suggestion)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        return View(new CreateTypeViewModel { DisplayName = suggestion, Name = suggestion.ToSafeName() });
    }

    [HttpPost, ActionName("Create")]
    public async Task<ActionResult> CreatePOST(CreateTypeViewModel viewModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        viewModel.DisplayName = viewModel.DisplayName?.Trim() ?? string.Empty;
        viewModel.Name ??= string.Empty;

        if (string.IsNullOrWhiteSpace(viewModel.DisplayName))
        {
            ModelState.AddModelError("DisplayName", S["The Display Name can't be empty."]);
        }
        var types = await _contentDefinitionService.LoadTypesAsync();

        if (types.Any(t => string.Equals(t.DisplayName.Trim(), viewModel.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("DisplayName", S["A type with the same Display Name already exists."]);
        }

        if (string.IsNullOrWhiteSpace(viewModel.Name))
        {
            ModelState.AddModelError("Name", S["The Technical Name can't be empty."]);
        }

        if (!string.IsNullOrWhiteSpace(viewModel.Name) && !char.IsLetter(viewModel.Name[0]))
        {
            ModelState.AddModelError("Name", S["The Technical Name must start with a letter."]);
        }

        if (!string.Equals(viewModel.Name, viewModel.Name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Name", S["The Technical Name contains invalid characters."]);
        }

        if (viewModel.Name.IsReservedContentName())
        {
            ModelState.AddModelError("Name", S["The Technical Name is reserved for internal use."]);
        }

        if (types.Any(t => string.Equals(t.Name.Trim(), viewModel.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("Name", S["A type with the same Technical Name already exists."]);
        }

        if (!ModelState.IsValid)
        {
            await _documentStore.CancelAsync();
            return View(viewModel);
        }

        var contentTypeDefinition = await _contentDefinitionService.AddTypeAsync(viewModel.Name, viewModel.DisplayName);

        var typeViewModel = new EditTypeViewModel(contentTypeDefinition);

        await _notifier.SuccessAsync(H["The \"{0}\" content type has been created.", typeViewModel.DisplayName]);

        return RedirectToAction("AddPartsTo", new { id = typeViewModel.Name });
    }

    [Admin("ContentTypes/Edit/{id}", "EditType")]
    public async Task<ActionResult> Edit(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var typeViewModel = await _contentDefinitionService.GetTypeAsync(id);

        if (typeViewModel == null)
        {
            return NotFound();
        }

        typeViewModel.Editor = await _contentDefinitionDisplayManager.BuildTypeEditorAsync(typeViewModel.TypeDefinition, _updateModelAccessor.ModelUpdater);

        return View(typeViewModel);
    }

    [HttpPost, ActionName("Edit")]
    [FormValueRequired("submit.Save")]
    public async Task<ActionResult> EditPOST(string id, EditTypeViewModel viewModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var contentTypeDefinition = await _contentDefinitionManager.LoadTypeDefinitionAsync(id);

        if (contentTypeDefinition == null)
        {
            return NotFound();
        }

        viewModel.Settings = contentTypeDefinition.Settings;
        viewModel.TypeDefinition = contentTypeDefinition;
        viewModel.DisplayName = contentTypeDefinition.DisplayName;
        viewModel.Editor = await _contentDefinitionDisplayManager.UpdateTypeEditorAsync(contentTypeDefinition, _updateModelAccessor.ModelUpdater);

        if (!ModelState.IsValid)
        {
            await _documentStore.CancelAsync();

            return View(viewModel);
        }
        else
        {
            var ownedPartDefinition = await _contentDefinitionManager.LoadPartDefinitionAsync(contentTypeDefinition.Name);
            if (ownedPartDefinition != null && viewModel.OrderedFieldNames != null)
            {
                await _contentDefinitionService.AlterPartFieldsOrderAsync(ownedPartDefinition, viewModel.OrderedFieldNames);
            }
            await _contentDefinitionService.AlterTypePartsOrderAsync(contentTypeDefinition, viewModel.OrderedPartNames);
            await _notifier.SuccessAsync(H["\"{0}\" settings have been saved.", contentTypeDefinition.Name]);
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost, ActionName("Edit")]
    [FormValueRequired("submit.Delete")]
    public async Task<ActionResult> Delete(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var typeViewModel = await _contentDefinitionService.LoadTypeAsync(id);

        if (typeViewModel == null)
        {
            return NotFound();
        }

        await _contentDefinitionService.RemoveTypeAsync(id, true);

        await _notifier.SuccessAsync(H["\"{0}\" has been removed.", typeViewModel.DisplayName]);

        return RedirectToAction(nameof(List));
    }

    [Admin("ContentTypes/AddPartsTo/{id}", "AddPartsTo")]
    public async Task<ActionResult> AddPartsTo(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var typeViewModel = await _contentDefinitionService.GetTypeAsync(id);

        if (typeViewModel == null)
        {
            return NotFound();
        }

        var typePartNames = new HashSet<string>(typeViewModel.TypeDefinition.Parts.Select(p => p.IsNamedPart() ? p.Name : p.PartDefinition.Name));

        var viewModel = new AddPartsViewModel
        {
            Type = typeViewModel,
            PartSelections = (await _contentDefinitionService.GetPartsAsync(metadataPartsOnly: false))
                .Where(cpd => !typePartNames.Contains(cpd.Name, StringComparer.OrdinalIgnoreCase) && cpd.PartDefinition != null && cpd.PartDefinition.GetSettings<ContentPartSettings>().Attachable)
                .Select(cpd => new PartSelectionViewModel { PartName = cpd.Name, PartDisplayName = cpd.DisplayName, PartDescription = cpd.Description })
                .ToList()
        };

        return View(viewModel);
    }

    [Admin("ContentTypes/AddReusablePartTo/{id}", "AddReusablePartTo")]
    public async Task<ActionResult> AddReusablePartTo(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var typeViewModel = await _contentDefinitionService.GetTypeAsync(id);

        if (typeViewModel == null)
        {
            return NotFound();
        }

        var reusableParts = (await _contentDefinitionService.GetPartsAsync(metadataPartsOnly: false))
                .Where(cpd => cpd.PartDefinition != null &&
                    cpd.PartDefinition.GetSettings<ContentPartSettings>().Attachable &&
                    cpd.PartDefinition.GetSettings<ContentPartSettings>().Reusable);

        var viewModel = new AddReusablePartViewModel
        {
            Type = typeViewModel,
            PartSelections = reusableParts
                .Select(cpd => new PartSelectionViewModel { PartName = cpd.Name, PartDisplayName = cpd.DisplayName, PartDescription = cpd.Description })
                .ToList(),
            SelectedPartName = reusableParts.FirstOrDefault()?.Name
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("AddPartsTo")]
    public async Task<ActionResult> AddPartsToPOST(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var typeViewModel = await _contentDefinitionService.LoadTypeAsync(id);

        if (typeViewModel == null)
        {
            return NotFound();
        }

        var viewModel = new AddPartsViewModel();
        if (!await TryUpdateModelAsync(viewModel))
        {
            return await AddPartsTo(id);
        }

        var partsToAdd = viewModel.PartSelections.Where(ps => ps.IsSelected).Select(ps => ps.PartName);
        foreach (var partToAdd in partsToAdd)
        {
            await _contentDefinitionService.AddPartToTypeAsync(partToAdd, typeViewModel.Name);
            await _notifier.SuccessAsync(H["The \"{0}\" part has been added.", partToAdd]);
        }

        if (!ModelState.IsValid)
        {
            await _documentStore.CancelAsync();
            return await AddPartsTo(id);
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost, ActionName("AddReusablePartTo")]
    public async Task<ActionResult> AddReusablePartToPOST(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var typeViewModel = await _contentDefinitionService.LoadTypeAsync(id);

        if (typeViewModel == null)
        {
            return NotFound();
        }

        var viewModel = new AddReusablePartViewModel();
        if (!await TryUpdateModelAsync(viewModel))
        {
            return await AddReusablePartTo(id);
        }

        viewModel.DisplayName = viewModel.DisplayName?.Trim() ?? string.Empty;
        viewModel.Name ??= string.Empty;

        if (string.IsNullOrWhiteSpace(viewModel.DisplayName))
        {
            ModelState.AddModelError("DisplayName", S["The Display Name can't be empty."]);
        }

        if (typeViewModel.TypeDefinition.Parts.Any(f => string.Equals(f.DisplayName().Trim(), viewModel.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("DisplayName", S["A part with the same Display Name already exists."]);
        }

        if (!string.IsNullOrWhiteSpace(viewModel.Name) && !char.IsLetter(viewModel.Name[0]))
        {
            ModelState.AddModelError("Name", S["The Technical Name must start with a letter."]);
        }

        if (!string.Equals(viewModel.Name, viewModel.Name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Name", S["The Technical Name contains invalid characters."]);
        }

        if (viewModel.Name.IsReservedContentName())
        {
            ModelState.AddModelError("Name", S["The Technical Name is reserved for internal use."]);
        }

        if (string.IsNullOrWhiteSpace(viewModel.Name))
        {
            ModelState.AddModelError("Name", S["The Technical Name can't be empty."]);
        }

        if (typeViewModel.TypeDefinition.Parts.Any(f => string.Equals(f.Name.Trim(), viewModel.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("Name", S["A part with the same Technical Name already exists."]);
        }

        if (!ModelState.IsValid)
        {
            await _documentStore.CancelAsync();
            return await AddReusablePartTo(id);
        }

        var partToAdd = viewModel.SelectedPartName;

        await _contentDefinitionService.AddReusablePartToTypeAsync(viewModel.Name, viewModel.DisplayName, viewModel.Description, partToAdd, typeViewModel.Name);

        await _notifier.SuccessAsync(H["The \"{0}\" part has been added.", partToAdd]);

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost, ActionName("RemovePart")]
    [Admin("ContentTypes/{id}/ContentParts/{name}/Remove", "RemovePart")]
    public async Task<ActionResult> RemovePart(string id, string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var typeViewModel = await _contentDefinitionService.LoadTypeAsync(id);

        if (typeViewModel == null)
        {
            return NotFound();
        }

        var partDefinition = typeViewModel.TypeDefinition.Parts.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

        if (partDefinition == null)
        {
            return NotFound();
        }

        await _contentDefinitionService.RemovePartFromTypeAsync(name, id);

        await _notifier.SuccessAsync(H["The \"{0}\" part has been removed.", name]);

        return RedirectToAction(nameof(Edit), new { id });
    }

    #endregion Types

    #region Parts

    [Admin("ContentTypes/ListParts", "ListContentParts")]
    public async Task<ActionResult> ListParts()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewContentTypes))
        {
            return Forbid();
        }

        return View(new ListContentPartsViewModel
        {
            // Only user-defined parts (not code as they are not configurable).
            Parts = await _contentDefinitionService.GetPartsAsync(metadataPartsOnly: true)
        });
    }

    [Admin("ContentParts/Create", "CreatePart")]
    public async Task<ActionResult> CreatePart(string suggestion)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        return View(new CreatePartViewModel { Name = suggestion.ToSafeName() });
    }

    [HttpPost, ActionName("CreatePart")]
    public async Task<ActionResult> CreatePartPOST(CreatePartViewModel viewModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        viewModel.Name ??= string.Empty;

        if (string.IsNullOrWhiteSpace(viewModel.Name))
        {
            ModelState.AddModelError("Name", S["The Technical Name can't be empty."]);
        }

        if ((await _contentDefinitionService.LoadPartsAsync(false)).Any(p => string.Equals(p.Name.Trim(), viewModel.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("Name", S["A part with the same Technical Name already exists."]);
        }

        if (!string.IsNullOrWhiteSpace(viewModel.Name) && !char.IsLetter(viewModel.Name[0]))
        {
            ModelState.AddModelError("Name", S["The Technical Name must start with a letter."]);
        }

        if (!string.Equals(viewModel.Name, viewModel.Name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Name", S["The Technical Name contains invalid characters."]);
        }

        if (viewModel.Name.IsReservedContentName())
        {
            ModelState.AddModelError("Name", S["The Technical Name is reserved for internal use."]);
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var partViewModel = await _contentDefinitionService.AddPartAsync(viewModel);

        if (partViewModel == null)
        {
            await _notifier.InformationAsync(H["The content part could not be created."]);
            return View(viewModel);
        }

        await _notifier.SuccessAsync(H["The \"{0}\" content part has been created.", partViewModel.Name]);

        return RedirectToAction(nameof(EditPart), new { id = partViewModel.Name });
    }

    [Admin("ContentParts/Edit/{id}", "EditPart")]
    public async Task<ActionResult> EditPart(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var contentPartDefinition = await _contentDefinitionManager.GetPartDefinitionAsync(id);

        if (contentPartDefinition == null)
        {
            return NotFound();
        }

        var viewModel = new EditPartViewModel(contentPartDefinition)
        {
            Editor = await _contentDefinitionDisplayManager.BuildPartEditorAsync(contentPartDefinition, _updateModelAccessor.ModelUpdater),
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("EditPart")]
    [FormValueRequired("submit.Save")]
    public async Task<ActionResult> EditPartPOST(string id, string[] orderedFieldNames)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var contentPartDefinition = await _contentDefinitionManager.LoadPartDefinitionAsync(id);

        if (contentPartDefinition == null)
        {
            return NotFound();
        }

        var viewModel = new EditPartViewModel(contentPartDefinition)
        {
            Editor = await _contentDefinitionDisplayManager.UpdatePartEditorAsync(contentPartDefinition, _updateModelAccessor.ModelUpdater),
        };

        if (!ModelState.IsValid)
        {
            await _documentStore.CancelAsync();
            return View(viewModel);
        }
        else
        {
            await _contentDefinitionService.AlterPartFieldsOrderAsync(contentPartDefinition, orderedFieldNames);
            await _notifier.SuccessAsync(H["The settings of \"{0}\" have been saved.", contentPartDefinition.Name]);
        }

        return RedirectToAction(nameof(EditPart), new { id });
    }

    [HttpPost, ActionName("EditPart")]
    [FormValueRequired("submit.Delete")]
    public async Task<ActionResult> DeletePart(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var partViewModel = await _contentDefinitionService.LoadPartAsync(id);

        if (partViewModel == null)
        {
            return NotFound();
        }

        await _contentDefinitionService.RemovePartAsync(id);

        await _notifier.InformationAsync(H["\"{0}\" has been removed.", partViewModel.DisplayName]);

        return RedirectToAction(nameof(ListParts));
    }

    [Admin("ContentTypes/AddFieldsTo/{id}", "AddFieldsTo")]
    public async Task<ActionResult> AddFieldTo(string id, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var fields = (await _contentDefinitionService.GetFieldsAsync()).ToList();

        if (fields.Count == 0)
        {
            await _notifier.WarningAsync(H["There are no fields."]);

            return RedirectToAction(nameof(List));
        }

        var partViewModel = await _contentDefinitionService.LoadPartAsync(id);

        if (partViewModel == null)
        {
            return NotFound();
        }

        var viewModel = new AddFieldViewModel
        {
            Part = partViewModel.PartDefinition,
            Fields = fields.Select(field => field.Name).OrderBy(name => name).ToList()
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

        var partViewModel = await _contentDefinitionService.LoadPartAsync(id);

        if (partViewModel == null)
        {
            return NotFound();
        }

        var fields = (await _contentDefinitionService.GetFieldsAsync()).ToList();

        if (!fields.Any(field => string.Equals(field.Name, viewModel.FieldTypeName, StringComparison.OrdinalIgnoreCase)))
        {
            return NotFound();
        }

        var partDefinition = partViewModel.PartDefinition;

        viewModel.DisplayName = viewModel.DisplayName?.Trim() ?? string.Empty;
        viewModel.Name ??= string.Empty;

        if (string.IsNullOrWhiteSpace(viewModel.DisplayName))
        {
            ModelState.AddModelError("DisplayName", S["The Display Name can't be empty."]);
        }

        if (partDefinition.Fields.Any(f => string.Equals(f.DisplayName().Trim(), viewModel.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("DisplayName", S["A field with the same Display Name already exists."]);
        }

        if (string.IsNullOrWhiteSpace(viewModel.Name))
        {
            ModelState.AddModelError("Name", S["The Technical Name can't be empty."]);
        }

        if (partDefinition.Fields.Any(f => string.Equals(f.Name.Trim(), viewModel.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("Name", S["A field with the same Technical Name already exists."]);
        }

        if (!string.IsNullOrWhiteSpace(viewModel.Name) && !char.IsLetter(viewModel.Name[0]))
        {
            ModelState.AddModelError("Name", S["The Technical Name must start with a letter."]);
        }

        if (!string.Equals(viewModel.Name, viewModel.Name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Name", S["The Technical Name contains invalid characters."]);
        }

        if (!ModelState.IsValid)
        {
            viewModel.Part = partDefinition;
            viewModel.Fields = (await _contentDefinitionService.GetFieldsAsync()).Select(x => x.Name).OrderBy(x => x).ToList();

            await _documentStore.CancelAsync();

            ViewData["ReturnUrl"] = returnUrl;
            return View(viewModel);
        }

        await _contentDefinitionService.AddFieldToPartAsync(viewModel.Name, viewModel.DisplayName, viewModel.FieldTypeName, partDefinition.Name);

        await _notifier.SuccessAsync(H["The field \"{0}\" has been added.", viewModel.DisplayName]);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return this.Redirect(returnUrl, true);
        }
        else
        {
            return RedirectToAction(nameof(EditField), new { id, viewModel.Name });
        }
    }

    [Admin("ContentParts/{id}/Fields/{name}/Edit", "EditField")]
    public async Task<ActionResult> EditField(string id, string name, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var partViewModel = await _contentDefinitionService.GetPartAsync(id);

        if (partViewModel == null)
        {
            return NotFound();
        }

        var partFieldDefinition = partViewModel.PartDefinition.Fields.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

        if (partFieldDefinition?.FieldDefinition?.Name == null
            || !(await _contentDefinitionService.GetFieldsAsync()).Any(field => string.Equals(field.Name, partFieldDefinition.FieldDefinition.Name, StringComparison.OrdinalIgnoreCase)))
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

        var partViewModel = await _contentDefinitionService.LoadPartAsync(id);

        if (partViewModel == null)
        {
            return NotFound();
        }

        var field = (await _contentDefinitionManager.LoadPartDefinitionAsync(id)).Fields.FirstOrDefault(x => string.Equals(x.Name, viewModel.Name, StringComparison.OrdinalIgnoreCase));

        if (field == null)
        {
            return NotFound();
        }

        viewModel.PartFieldDefinition = field;

        if (field.DisplayName() != viewModel.DisplayName)
        {
            // prevent null reference exception in validation
            viewModel.DisplayName = viewModel.DisplayName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(viewModel.DisplayName))
            {
                ModelState.AddModelError("DisplayName", S["The Display Name can't be empty."]);
            }

            if ((await _contentDefinitionService.LoadPartAsync(partViewModel.Name)).PartDefinition.Fields.Any(t => t.Name != viewModel.Name && string.Equals(t.DisplayName().Trim(), viewModel.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase)))
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

        await _contentDefinitionService.AlterFieldAsync(partViewModel, viewModel);

        // Refresh the local field variable in case it has been altered
        field = (await _contentDefinitionManager.LoadPartDefinitionAsync(id)).Fields.FirstOrDefault(x => string.Equals(x.Name, viewModel.Name, StringComparison.OrdinalIgnoreCase));

        viewModel.Shape = await _contentDefinitionDisplayManager.UpdatePartFieldEditorAsync(field, _updateModelAccessor.ModelUpdater);

        if (!ModelState.IsValid)
        {
            await _documentStore.CancelAsync();

            ViewData["ReturnUrl"] = returnUrl;
            return View(viewModel);
        }
        else
        {
            await _notifier.SuccessAsync(H["The \"{0}\" field settings have been saved.", field.DisplayName()]);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return this.Redirect(returnUrl, true);
        }
        else
        {
            // Redirect to the type editor if a type exists with this name
            var typeViewModel = await _contentDefinitionService.LoadTypeAsync(id);
            if (typeViewModel != null)
            {
                return RedirectToAction(nameof(Edit), new { id });
            }

            return RedirectToAction(nameof(EditPart), new { id });
        }
    }

    [HttpPost, ActionName("RemoveFieldFrom")]
    public async Task<ActionResult> RemoveFieldFromPOST(string id, string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var partViewModel = await _contentDefinitionService.LoadPartAsync(id);

        if (partViewModel == null)
        {
            return NotFound();
        }

        var field = partViewModel.PartDefinition.Fields.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

        if (field == null)
        {
            return NotFound();
        }

        await _contentDefinitionService.RemoveFieldFromPartAsync(name, partViewModel.Name);

        await _notifier.SuccessAsync(H["The \"{0}\" field has been removed.", field.DisplayName()]);

        if (await _contentDefinitionService.LoadTypeAsync(id) != null)
        {
            return RedirectToAction(nameof(Edit), new { id });
        }

        return RedirectToAction(nameof(EditPart), new { id });
    }

    #endregion Parts

    #region Type Parts

    [Admin("ContentTypes/{id}/ContentParts/{name}/Edit", "EditTypePart")]
    public async Task<ActionResult> EditTypePart(string id, string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(id);

        if (typeDefinition == null)
        {
            return NotFound();
        }

        var typePartDefinition = typeDefinition.Parts.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

        if (typePartDefinition == null)
        {
            return NotFound();
        }

        var typePartViewModel = new EditTypePartViewModel
        {
            Name = typePartDefinition.Name,
            Editor = typePartDefinition.Editor(),
            DisplayMode = typePartDefinition.DisplayMode(),
            DisplayName = typePartDefinition.DisplayName(),
            Description = typePartDefinition.Description(),
            TypePartDefinition = typePartDefinition,
            Shape = await _contentDefinitionDisplayManager.BuildTypePartEditorAsync(typePartDefinition, _updateModelAccessor.ModelUpdater)
        };

        return View(typePartViewModel);
    }

    [HttpPost, ActionName("EditTypePart")]
    [FormValueRequired("submit.Save")]
    public async Task<ActionResult> EditTypePartPOST(string id, EditTypePartViewModel viewModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
        {
            return Forbid();
        }

        if (viewModel == null)
        {
            return NotFound();
        }

        var typeDefinition = await _contentDefinitionManager.LoadTypeDefinitionAsync(id);

        if (typeDefinition == null)
        {
            return NotFound();
        }

        var part = typeDefinition.Parts.FirstOrDefault(x => string.Equals(x.Name, viewModel.Name, StringComparison.OrdinalIgnoreCase));

        if (part == null)
        {
            return NotFound();
        }

        viewModel.TypePartDefinition = part;

        if (part.PartDefinition.IsReusable())
        {
            if (part.DisplayName() != viewModel.DisplayName)
            {
                // Prevent null reference exception in validation
                viewModel.DisplayName = viewModel.DisplayName?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(viewModel.DisplayName))
                {
                    ModelState.AddModelError("DisplayName", S["The Display Name can't be empty."]);
                }

                if (typeDefinition.Parts.Any(t => t.Name != viewModel.Name && string.Equals(t.DisplayName()?.Trim(), viewModel.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("DisplayName", S["A part with the same Display Name already exists."]);
                }

                if (!ModelState.IsValid)
                {
                    viewModel.Shape = await _contentDefinitionDisplayManager.UpdateTypePartEditorAsync(part, _updateModelAccessor.ModelUpdater);
                    await _documentStore.CancelAsync();
                    return View(viewModel);
                }
            }
        }

        await _contentDefinitionService.AlterTypePartAsync(viewModel);

        // Refresh the local part variable in case it has been altered
        part = (await _contentDefinitionManager.LoadTypeDefinitionAsync(id)).Parts.FirstOrDefault(x => string.Equals(x.Name, viewModel.Name, StringComparison.OrdinalIgnoreCase));

        viewModel.Shape = await _contentDefinitionDisplayManager.UpdateTypePartEditorAsync(part, _updateModelAccessor.ModelUpdater);

        if (!ModelState.IsValid)
        {
            await _documentStore.CancelAsync();
            return View(viewModel);
        }
        else
        {
            await _notifier.SuccessAsync(H["The \"{0}\" part settings have been saved.", part.DisplayName()]);
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    #endregion Type Parts
}
