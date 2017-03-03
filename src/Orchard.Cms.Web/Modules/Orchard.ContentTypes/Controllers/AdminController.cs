using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentTypes.Editors;
using Orchard.ContentTypes.Services;
using Orchard.ContentTypes.ViewModels;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Notify;
using Orchard.Environment.Shell;
using Microsoft.AspNetCore.Mvc.Modules.Utilities;
using YesSql.Core.Services;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Modules.ActionConstraints;

namespace Orchard.ContentTypes.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ShellSettings _settings;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISession _session;
        private readonly IContentDefinitionDisplayManager _contentDefinitionDisplayManager;
        private readonly INotifier _notifier;

        public AdminController(
            IContentDefinitionDisplayManager contentDefinitionDisplayManager,
            IContentDefinitionService contentDefinitionService,
            IContentDefinitionManager contentDefinitionManager,
            ShellSettings settings,
            IAuthorizationService authorizationService,
            ISession session,
            ILogger<AdminController> logger,
            IHtmlLocalizer<AdminMenu> htmlLocalizer,
            IStringLocalizer<AdminMenu> stringLocalizer,
            INotifier notifier
            )
        {
            _notifier = notifier;
            _contentDefinitionDisplayManager = contentDefinitionDisplayManager;
            _session = session;
            _authorizationService = authorizationService;
            _contentDefinitionService = contentDefinitionService;
            _contentDefinitionManager = contentDefinitionManager;
            _settings = settings;

            Logger = logger;
            T = htmlLocalizer;
            S = stringLocalizer;
        }

        public IHtmlLocalizer T { get; set; }
        public IStringLocalizer S { get; set; }
        public ILogger Logger { get; set; }
        public Task<ActionResult> Index() { return List(); }

        #region Types

        public async Task<ActionResult> List()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewContentTypes))
                return Unauthorized();

            return View("List", new ListContentTypesViewModel
            {
                Types = _contentDefinitionService.GetTypes()
            });
        }

        public async Task<ActionResult> Create(string suggestion)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            return View(new CreateTypeViewModel { DisplayName = suggestion, Name = suggestion.ToSafeName() });
        }

        [HttpPost, ActionName("Create")]
        public async Task<ActionResult> CreatePOST(CreateTypeViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            viewModel.DisplayName = viewModel.DisplayName ?? String.Empty;
            viewModel.Name = viewModel.Name ?? String.Empty;

            if (String.IsNullOrWhiteSpace(viewModel.DisplayName))
            {
                ModelState.AddModelError("DisplayName", S["The Display Name can't be empty."]);
            }

            if (String.IsNullOrWhiteSpace(viewModel.Name))
            {
                ModelState.AddModelError("Name", S["The Content Type Id can't be empty."]);
            }

            if (_contentDefinitionService.GetTypes().Any(t => String.Equals(t.Name.Trim(), viewModel.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("Name", S["A type with the same Id already exists."]);
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Name) && !viewModel.Name[0].IsLetter())
            {
                ModelState.AddModelError("Name", S["The technical name must start with a letter."]);
            }

            if (_contentDefinitionService.GetTypes().Any(t => String.Equals(t.DisplayName.Trim(), viewModel.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("DisplayName", S["A type with the same Display Name already exists."]);
            }

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return View(viewModel);
            }

            var contentTypeDefinition = _contentDefinitionService.AddType(viewModel.Name, viewModel.DisplayName);

            var typeViewModel = new EditTypeViewModel(contentTypeDefinition);


            _notifier.Success(T["The \"{0}\" content type has been created.", typeViewModel.DisplayName]);

            return RedirectToAction("AddPartsTo", new { id = typeViewModel.Name });
        }

        public async Task<ActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var typeViewModel = _contentDefinitionService.GetType(id);

            if (typeViewModel == null)
            {
                return NotFound();
            }

            typeViewModel.Editor = await _contentDefinitionDisplayManager.BuildTypeEditorAsync(typeViewModel.TypeDefinition, this);

            return View(typeViewModel);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public async Task<ActionResult> EditPOST(string id, EditTypeViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(id);

            if (contentTypeDefinition == null)
            {
                return NotFound();
            }

            viewModel.Settings = contentTypeDefinition.Settings;
            viewModel.TypeDefinition = contentTypeDefinition;
            viewModel.DisplayName = contentTypeDefinition.DisplayName;
            viewModel.Editor = await _contentDefinitionDisplayManager.UpdateTypeEditorAsync(contentTypeDefinition, this);

            if (!ModelState.IsValid)
            {
                _session.Cancel();

                HackModelState(nameof(EditTypeViewModel.OrderedFieldNames));
                HackModelState(nameof(EditTypeViewModel.OrderedPartNames));

                return View(viewModel);
            }
            else
            {
                var ownedPartDefinition = _contentDefinitionManager.GetPartDefinition(contentTypeDefinition.Name);
                if (ownedPartDefinition != null && viewModel.OrderedFieldNames != null)
                {
                    _contentDefinitionService.AlterPartFieldsOrder(ownedPartDefinition, viewModel.OrderedFieldNames);
                }
                _contentDefinitionService.AlterTypePartsOrder(contentTypeDefinition, viewModel.OrderedPartNames);
                _notifier.Success(T["\"{0}\" settings have been saved.", contentTypeDefinition.Name]);
            }

            return RedirectToAction("Edit", new { id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Delete")]
        public async Task<ActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var typeViewModel = _contentDefinitionService.GetType(id);

            if (typeViewModel == null)
                return NotFound();

            _contentDefinitionService.RemoveType(id, true);

            _notifier.Success(T["\"{0}\" has been removed.", typeViewModel.DisplayName]);

            return RedirectToAction("List");
        }

        public async Task<ActionResult> AddPartsTo(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
            {
                return Unauthorized();
            }

            var typeViewModel = _contentDefinitionService.GetType(id);

            if (typeViewModel == null)
                return NotFound();

            var typePartNames = new HashSet<string>(
                typeViewModel.TypeDefinition.Parts
                    .Where(cpd => !cpd.Settings.ToObject<ContentPartSettings>().Reusable)
                    .Select(p => p.PartDefinition.Name)
                );

            var viewModel = new AddPartsViewModel
            {
                Type = typeViewModel,
                PartSelections = _contentDefinitionService.GetParts(metadataPartsOnly: false)
                    .Where(cpd => !typePartNames.Contains(cpd.Name) &&
                        cpd.Settings.ToObject<ContentPartSettings>().Attachable &&
                        !cpd.Settings.ToObject<ContentPartSettings>().Reusable)
                    .Select(cpd => new PartSelectionViewModel { PartName = cpd.Name, PartDisplayName = cpd.DisplayName, PartDescription = cpd.Description })
                    .ToList()
            };

            return View(viewModel);
        }

        public async Task<ActionResult> AddReusablePartTo(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
            {
                return Unauthorized();
            }

            var typeViewModel = _contentDefinitionService.GetType(id);

            if (typeViewModel == null)
                return NotFound();

            var reusableParts = _contentDefinitionService.GetParts(metadataPartsOnly: false)
                    .Where(cpd =>
                        cpd.Settings.ToObject<ContentPartSettings>().Attachable &&
                        cpd.Settings.ToObject<ContentPartSettings>().Reusable);

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
                return Unauthorized();

            var typeViewModel = _contentDefinitionService.GetType(id);

            if (typeViewModel == null)
                return NotFound();

            var viewModel = new AddPartsViewModel();
            if (!await TryUpdateModelAsync(viewModel))
                return await AddPartsTo(id);

            var partsToAdd = viewModel.PartSelections.Where(ps => ps.IsSelected).Select(ps => ps.PartName);
            foreach (var partToAdd in partsToAdd)
            {
                _contentDefinitionService.AddPartToType(partToAdd, typeViewModel.Name);
                _notifier.Success(T["The \"{0}\" part has been added.", partToAdd]);
            }

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return await AddPartsTo(id);
            }

            return RedirectToAction("Edit", new { id });
        }

        [HttpPost, ActionName("AddReusablePartTo")]
        public async Task<ActionResult> AddReusablePartToPOST(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var typeViewModel = _contentDefinitionService.GetType(id);

            if (typeViewModel == null)
                return NotFound();

            var viewModel = new AddReusablePartViewModel();
            if (!await TryUpdateModelAsync(viewModel))
            {
                return await AddReusablePartTo(id);
            }

            if (String.IsNullOrWhiteSpace(viewModel.DisplayName))
            {
                ModelState.AddModelError("DisplayName", S["The Display Name can't be empty."]);
            }

            if (String.IsNullOrWhiteSpace(viewModel.Name))
            {
                ModelState.AddModelError("Name", S["The Content Type Id can't be empty."]);
            }

            var partToAdd = viewModel.SelectedPartName;

            _contentDefinitionService.AddReusablePartToType(viewModel.Name, viewModel.DisplayName, viewModel.Description, partToAdd, typeViewModel.Name);
            _notifier.Success(T["The \"{0}\" part has been added.", partToAdd]);

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return await AddReusablePartTo(id);
            }

            return RedirectToAction("Edit", new { id });
        }

        [HttpPost, ActionName("RemovePart")]
        public async Task<ActionResult> RemovePartPOST(string id, string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var typeViewModel = _contentDefinitionService.GetType(id);

            if (typeViewModel == null
                || !typeViewModel.TypeDefinition.Parts.Any(p => p.Name == name))
                return NotFound();

            _contentDefinitionService.RemovePartFromType(name, id);

            _notifier.Success(T["The \"{0}\" part has been removed.", name]);

            return RedirectToAction("Edit", new { id });
        }

        #endregion

        #region Parts

        public ActionResult ListParts()
        {
            return View(new ListContentPartsViewModel
            {
                // only user-defined parts (not code as they are not configurable)
                Parts = _contentDefinitionService.GetParts(true/*metadataPartsOnly*/)
            });
        }

        public async Task<ActionResult> CreatePart(string suggestion)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            return View(new CreatePartViewModel { Name = suggestion.ToSafeName() });
        }

        [HttpPost, ActionName("CreatePart")]
        public async Task<ActionResult> CreatePartPOST(CreatePartViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            if (_contentDefinitionManager.GetPartDefinition(viewModel.Name) != null)
                ModelState.AddModelError("Name", S["Cannot add part named '{0}'. It already exists.", viewModel.Name]);

            if (!ModelState.IsValid)
                return View(viewModel);

            var partViewModel = _contentDefinitionService.AddPart(viewModel);

            if (partViewModel == null)
            {
                _notifier.Information(T["The content part could not be created."]);
                return View(viewModel);
            }

            _notifier.Success(T["The \"{0}\" content part has been created.", partViewModel.Name]);

            return RedirectToAction("EditPart", new { id = partViewModel.Name });
        }

        public async Task<ActionResult> EditPart(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var contentPartDefinition = _contentDefinitionManager.GetPartDefinition(id);

            if (contentPartDefinition == null)
            {
                return NotFound();
            }

            var viewModel = new EditPartViewModel(contentPartDefinition);
            viewModel.Editor = await _contentDefinitionDisplayManager.BuildPartEditorAsync(contentPartDefinition, this);

            return View(viewModel);
        }

        [HttpPost, ActionName("EditPart")]
        [FormValueRequired("submit.Save")]
        public async Task<ActionResult> EditPartPOST(string id, string[] orderedFieldNames)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var contentPartDefinition = _contentDefinitionManager.GetPartDefinition(id);

            if (contentPartDefinition == null)
            {
                return NotFound();
            }

            var viewModel = new EditPartViewModel(contentPartDefinition);
            viewModel.Editor = await _contentDefinitionDisplayManager.UpdatePartEditorAsync(contentPartDefinition, this);

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return View(viewModel);
            }
            else
            {
                _contentDefinitionService.AlterPartFieldsOrder(contentPartDefinition, orderedFieldNames);
                _notifier.Success(T["The settings of \"{0}\" have been saved.", contentPartDefinition.Name]);
            }

            return RedirectToAction("EditPart", new { id });
        }

        [HttpPost, ActionName("EditPart")]
        [FormValueRequired("submit.Delete")]
        public async Task<ActionResult> DeletePart(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var partViewModel = _contentDefinitionService.GetPart(id);

            if (partViewModel == null)
                return NotFound();

            _contentDefinitionService.RemovePart(id);

            _notifier.Information(T["\"{0}\" has been removed.", partViewModel.DisplayName]);

            return RedirectToAction("ListParts");
        }

        public async Task<ActionResult> AddFieldTo(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var partViewModel = _contentDefinitionService.GetPart(id);

            if (partViewModel == null)
            {
                return NotFound();
            }

            var viewModel = new AddFieldViewModel
            {
                Part = partViewModel.PartDefinition,
                Fields = _contentDefinitionService.GetFields().Select(x => x.Name).OrderBy(x => x).ToList()
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("AddFieldTo")]
        public async Task<ActionResult> AddFieldToPOST(AddFieldViewModel viewModel, string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var partViewModel = _contentDefinitionService.GetPart(id);

            if (partViewModel == null)
            {
                return NotFound();
            }

            var partDefinition = partViewModel.PartDefinition;

            viewModel.DisplayName = viewModel.DisplayName?.Trim() ?? String.Empty;

            viewModel.Name = viewModel.Name ?? String.Empty;

            if (String.IsNullOrWhiteSpace(viewModel.DisplayName))
            {
                ModelState.AddModelError("DisplayName", S["The Display Name name can't be empty."]);
            }

            if (String.IsNullOrWhiteSpace(viewModel.Name))
            {
                ModelState.AddModelError("Name", S["The Technical Name can't be empty."]);
            }

            if (partDefinition.Fields.Any(t => String.Equals(t.Name.Trim(), viewModel.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("Name", S["A field with the same name already exists."]);
            }

            if (!String.IsNullOrWhiteSpace(viewModel.Name) && !viewModel.Name[0].IsLetter())
            {
                ModelState.AddModelError("Name", S["The technical name must start with a letter."]);
            }

            if (!String.Equals(viewModel.Name, viewModel.Name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Name", S["The technical name contains invalid characters."]);
            }

            if (partDefinition.Fields.Any(t => String.Equals(t.DisplayName().Trim(), Convert.ToString(viewModel.DisplayName).Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("DisplayName", S["A field with the same Display Name already exists."]);
            }

            if (!ModelState.IsValid)
            {
                viewModel.Part = partDefinition;
                viewModel.Fields = _contentDefinitionService.GetFields().Select(x => x.Name).OrderBy(x => x).ToList();

                _session.Cancel();

                return View(viewModel);
            }

            _contentDefinitionService.AddFieldToPart(viewModel.Name, viewModel.DisplayName, viewModel.FieldTypeName, partDefinition.Name);

            _notifier.Success(T["The field \"{0}\" has been added.", viewModel.DisplayName]);

            return RedirectToAction("EditField", new { id, viewModel.Name });
        }

        public async Task<ActionResult> EditField(string id, string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var partViewModel = _contentDefinitionService.GetPart(id);

            if (partViewModel == null)
            {
                return NotFound();
            }

            var partFieldDefinition = partViewModel.PartDefinition.Fields.FirstOrDefault(x => x.Name == name);

            if (partFieldDefinition == null)
            {
                return NotFound();
            }

            var viewModel = new EditFieldViewModel
            {
                Name = partFieldDefinition.Name,
                DisplayName = partFieldDefinition.DisplayName(),
                PartFieldDefinition = partFieldDefinition,
                Editor = await _contentDefinitionDisplayManager.BuildPartFieldEditorAsync(partFieldDefinition, this)
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("EditField")]
        [FormValueRequired("submit.Save")]
        public async Task<ActionResult> EditFieldPOST(string id, EditFieldViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            if (viewModel == null)
                return NotFound();

            var partViewModel = _contentDefinitionService.GetPart(id);

            if (partViewModel == null)
            {
                return NotFound();
            }

            var field = _contentDefinitionManager.GetPartDefinition(id).Fields.FirstOrDefault(x => x.Name == viewModel.Name);

            if (field == null)
            {
                return NotFound();
            }

            if (field.DisplayName() != viewModel.DisplayName)
            {
                // prevent null reference exception in validation
                viewModel.DisplayName = viewModel.DisplayName?.Trim() ?? String.Empty;

                if (String.IsNullOrWhiteSpace(viewModel.DisplayName))
                {
                    ModelState.AddModelError("DisplayName", S["The Display Name name can't be empty."]);
                }

                if (_contentDefinitionService.GetPart(partViewModel.Name).PartDefinition.Fields.Any(t => t.Name != viewModel.Name && String.Equals(t.DisplayName().Trim(), viewModel.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("DisplayName", S["A field with the same Display Name already exists."]);
                }

                if (!ModelState.IsValid)
                {
                    _session.Cancel();
                    return View(viewModel);
                }

                _contentDefinitionService.AlterField(partViewModel, viewModel);

                _notifier.Information(T["Display name changed to {0}.", viewModel.DisplayName]);
            }

            viewModel.Editor = await _contentDefinitionDisplayManager.UpdatePartFieldEditorAsync(field, this);

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return View(viewModel);
            }
            else
            {
                _notifier.Success(T["The \"{0}\" field settings have been saved.", field.DisplayName()]);
            }

            // Redirect to the type editor if a type exists with this name
            var typeViewModel = _contentDefinitionService.GetType(id);
            if (typeViewModel != null)
            {
                return RedirectToAction("Edit", new { id });
            }

            return RedirectToAction("EditPart", new { id });
        }

        [HttpPost, ActionName("RemoveFieldFrom")]
        public async Task<ActionResult> RemoveFieldFromPOST(string id, string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var partViewModel = _contentDefinitionService.GetPart(id);

            if (partViewModel == null)
            {
                return NotFound();
            }

            var field = partViewModel.PartDefinition.Fields.FirstOrDefault(x => x.Name == name);

            if (field == null)
            {
                return NotFound();
            }

            _contentDefinitionService.RemoveFieldFromPart(name, partViewModel.Name);

            _notifier.Success(T["The \"{0}\" field has been removed.", field.DisplayName()]);

            if (_contentDefinitionService.GetType(id) != null)
            {
                return RedirectToAction("Edit", new { id });
            }

            return RedirectToAction("EditPart", new { id });
        }

        #endregion

        #region Type Parts
        public async Task<ActionResult> EditTypePart(string id, string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(id);

            if (typeDefinition == null)
            {
                return NotFound();
            }

            var typePartDefinition = typeDefinition.Parts.FirstOrDefault(x => x.Name == name);

            if (typePartDefinition == null)
            {
                return NotFound();
            }

            var typePartViewModel = new EditTypePartViewModel
            {
                Name = typePartDefinition.Name,
                DisplayName = typePartDefinition.DisplayName(),
                Description = typePartDefinition.Description(),
                TypePartDefinition = typePartDefinition,
                Editor = await _contentDefinitionDisplayManager.BuildTypePartEditorAsync(typePartDefinition, this)
            };

            return View(typePartViewModel);
        }

        [HttpPost, ActionName("EditTypePart")]
        [FormValueRequired("submit.Save")]
        public async Task<ActionResult> EditTypePartPOST(string id, EditTypePartViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            if (viewModel == null)
            {
                return NotFound();
            }

            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(id);

            if (typeDefinition == null)
            {
                return NotFound();
            }

            var part = typeDefinition.Parts.FirstOrDefault(x => x.Name == viewModel.Name);

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
                    viewModel.DisplayName = viewModel.DisplayName?.Trim() ?? String.Empty;

                    if (String.IsNullOrWhiteSpace(viewModel.DisplayName))
                    {
                        ModelState.AddModelError("DisplayName", S["The display name can't be empty."]);
                    }

                    if (typeDefinition.Parts.Any(t => t.Name != viewModel.Name && String.Equals(t.DisplayName()?.Trim(), viewModel.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        ModelState.AddModelError("DisplayName", S["A part with the same display name already exists."]);
                    }

                    if (!ModelState.IsValid)
                    {
                        _session.Cancel();
                        return View(viewModel);
                    }

                }

                _contentDefinitionService.AlterTypePart(viewModel);
            }

            viewModel.Editor = await _contentDefinitionDisplayManager.UpdateTypePartEditorAsync(part, this);

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return View(viewModel);
            }
            else
            {
                _notifier.Success(T["The \"{0}\" part settings have been saved.", part.DisplayName()]);
            }

            return RedirectToAction("Edit", new { id });
        }

        #endregion

        private void HackModelState(string key)
        {
            // TODO: Remove this once https://github.com/aspnet/Mvc/issues/4989 has shipped
            var modelStateEntry = ModelState[key];
            var nodeType = modelStateEntry.GetType();
            nodeType.GetMethod("GetNode").Invoke(modelStateEntry, new object[] { new Microsoft.Extensions.Primitives.StringSegment("--!!f-a-k-e"), true });
            ((System.Collections.IList)nodeType.GetProperty("ChildNodes").GetValue(modelStateEntry)).Clear();
        }
    }
}
