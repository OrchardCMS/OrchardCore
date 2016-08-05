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
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Editors;
using Orchard.ContentTypes.Services;
using Orchard.ContentTypes.ViewModels;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Notify;
using Orchard.Environment.Shell;
using Orchard.Mvc;
using Orchard.Utility;
using YesSql.Core.Services;

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

        public ActionResult ContentTypeName(string displayName, int version)
        {
            return Json(new
            {
                result = _contentDefinitionService.GenerateContentTypeNameFromDisplayName(displayName),
                version = version
            });
        }

        public ActionResult FieldName(string partName, string displayName, int version)
        {
            return Json(new
            {
                result = _contentDefinitionService.GenerateFieldNameFromDisplayName(partName, displayName),
                version = version
            });
        }

        public async Task<ActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var contentTypeDefinition =_contentDefinitionManager.GetTypeDefinition(id);

            if (contentTypeDefinition == null)
            {
                return NotFound();
            }

            var shape = await _contentDefinitionDisplayManager.BuildTypeEditorAsync(contentTypeDefinition, this);

            return View(shape);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public async Task<ActionResult> EditPOST(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(id);

            if (contentTypeDefinition == null)
            {
                return NotFound();
            }

            var shape = await _contentDefinitionDisplayManager.UpdateTypeEditorAsync(contentTypeDefinition, this);

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return View(shape);
            }
            else
            {
                _notifier.Success(T["\"{0}\" settings have been saved.", contentTypeDefinition.Name]);
            }

            return View(shape);

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

        public async Task<ActionResult> RemovePartFrom(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var typeViewModel = _contentDefinitionService.GetType(id);

            var viewModel = new RemovePartViewModel();
            if (typeViewModel == null
                || !await TryUpdateModelAsync(viewModel)
                || !typeViewModel.TypeDefinition.Parts.Any(p => p.PartDefinition.Name == viewModel.Name))
                return NotFound();

            viewModel.Type = typeViewModel;
            return View(viewModel);
        }

        [HttpPost, ActionName("RemovePartFrom")]
        public async Task<ActionResult> RemovePartFromPOST(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var typeViewModel = _contentDefinitionService.GetType(id);

            var viewModel = new RemovePartViewModel();
            if (typeViewModel == null
                || !await TryUpdateModelAsync(viewModel)
                || !typeViewModel.TypeDefinition.Parts.Any(p => p.Name == viewModel.Name))
                return NotFound();

            _contentDefinitionService.RemovePartFromType(viewModel.Name, typeViewModel.Name);

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                viewModel.Type = typeViewModel;
                return View(viewModel);
            }

            _notifier.Success(T["The \"{0}\" part has been removed.", viewModel.Name]);

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

            var shape = await _contentDefinitionDisplayManager.BuildPartEditorAsync(contentPartDefinition, this);

            return View(shape);
        }

        [HttpPost, ActionName("EditPart")]
        [FormValueRequired("submit.Save")]
        public async Task<ActionResult> EditPartPOST(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var contentPartDefinition = _contentDefinitionManager.GetPartDefinition(id);

            if (contentPartDefinition == null)
            {
                return NotFound();
            }

            var shape = await _contentDefinitionDisplayManager.UpdatePartEditorAsync(contentPartDefinition, this);

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return View(shape);
            }
            else
            {
                _notifier.Success(T["\"{0}\" settings have been saved.", contentPartDefinition.Name]);
            }

            return View(shape);
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
                //id passed in might be that of a type w/ no implicit field
                var typeViewModel = _contentDefinitionService.GetType(id);
                if (typeViewModel != null)
                    partViewModel = new EditPartViewModel(new ContentPartDefinition(id));
                else
                    return NotFound();
            }

            var viewModel = new AddFieldViewModel
            {
                Part = partViewModel,
                Fields = _contentDefinitionService.GetFields().OrderBy(x => x.FieldTypeName).ToList()
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("AddFieldTo")]
        public async Task<ActionResult> AddFieldToPOST(AddFieldViewModel viewModel, string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var partViewModel = _contentDefinitionService.GetPart(id);
            var typeViewModel = _contentDefinitionService.GetType(id);
            if (partViewModel == null)
            {
                // id passed in might be that of a type w/ no implicit field
                if (typeViewModel != null)
                {
                    partViewModel = new EditPartViewModel { Name = typeViewModel.Name };
                    _contentDefinitionService.AddPart(new CreatePartViewModel { Name = partViewModel.Name });
                    _contentDefinitionService.AddPartToType(partViewModel.Name, typeViewModel.Name);
                }
                else {
                    return NotFound();
                }
            }

            viewModel.DisplayName = viewModel.DisplayName ?? String.Empty;
            viewModel.DisplayName = viewModel.DisplayName.Trim();
            viewModel.Name = viewModel.Name ?? String.Empty;

            if (String.IsNullOrWhiteSpace(viewModel.DisplayName))
            {
                ModelState.AddModelError("DisplayName", S["The Display Name name can't be empty."]);
            }

            if (String.IsNullOrWhiteSpace(viewModel.Name))
            {
                ModelState.AddModelError("Name", S["The Technical Name can't be empty."]);
            }

            if (_contentDefinitionService.GetPart(partViewModel.Name).PartDefinition.Fields.Any(t => String.Equals(t.Name.Trim(), viewModel.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
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

            if (_contentDefinitionService.GetPart(partViewModel.Name).PartDefinition.Fields.Any(t => String.Equals(t.DisplayName.Trim(), Convert.ToString(viewModel.DisplayName).Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("DisplayName", S["A field with the same Display Name already exists."]);
            }

            if (!ModelState.IsValid)
            {
                viewModel.Part = partViewModel;
                viewModel.Fields = _contentDefinitionService.GetFields();

                _session.Cancel();

                return View(viewModel);
            }

            try
            {
                _contentDefinitionService.AddFieldToPart(viewModel.Name, viewModel.DisplayName, viewModel.FieldTypeName, partViewModel.Name);
            }
            catch
            {
                //Services.Notifier.Information(T("The \"{0}\" field was not added. {1}", viewModel.DisplayName, ex.Message));
                _session.Cancel();
                return await AddFieldTo(id);
            }

            _notifier.Success(T["The \"{0}\" field has been added.", viewModel.DisplayName]);

            if (typeViewModel != null)
            {
                return RedirectToAction("Edit", new { id });
            }

            return RedirectToAction("EditPart", new { id });
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

            var fieldViewModel = partViewModel.PartDefinition.Fields.FirstOrDefault(x => x.Name == name);

            if (fieldViewModel == null)
            {
                return NotFound();
            }

            var viewModel = new EditFieldNameViewModel
            {
                Name = fieldViewModel.Name,
                DisplayName = fieldViewModel.DisplayName
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("EditField")]
        [FormValueRequired("submit.Save")]
        public async Task<ActionResult> EditFieldPOST(string id, EditFieldNameViewModel viewModel)
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

            // prevent null reference exception in validation
            viewModel.DisplayName = viewModel.DisplayName ?? String.Empty;

            // remove extra spaces
            viewModel.DisplayName = viewModel.DisplayName.Trim();

            if (String.IsNullOrWhiteSpace(viewModel.DisplayName))
            {
                ModelState.AddModelError("DisplayName", S["The Display Name name can't be empty."]);
            }

            if (_contentDefinitionService.GetPart(partViewModel.Name).PartDefinition.Fields.Any(t => t.Name != viewModel.Name && String.Equals(t.DisplayName.Trim(), viewModel.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("DisplayName", S["A field with the same Display Name already exists."]);
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var field = _contentDefinitionManager.GetPartDefinition(id).Fields.FirstOrDefault(x => x.Name == viewModel.Name);

            if (field == null)
            {
                return NotFound();
            }

            _contentDefinitionService.AlterField(partViewModel, viewModel);

            _notifier.Information(T["Display name changed to {0}.", viewModel.DisplayName]);

            // redirect to the type editor if a type exists with this name
            var typeViewModel = _contentDefinitionService.GetType(id);
            if (typeViewModel != null)
            {
                return RedirectToAction("Edit", new { id });
            }

            return RedirectToAction("EditPart", new { id });
        }

        public async Task<ActionResult> RemoveFieldFrom(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var partViewModel = _contentDefinitionService.GetPart(id);

            var viewModel = new RemoveFieldViewModel();
            if (partViewModel == null
                || !await TryUpdateModelAsync(viewModel)
                || !partViewModel.PartDefinition.Fields.Any(p => p.Name == viewModel.Name))
                return NotFound();

            viewModel.Part = partViewModel;
            return View(viewModel);
        }

        [HttpPost, ActionName("RemoveFieldFrom")]
        public async Task<ActionResult> RemoveFieldFromPOST(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContentTypes))
                return Unauthorized();

            var partViewModel = _contentDefinitionService.GetPart(id);

            var viewModel = new RemoveFieldViewModel();
            if (partViewModel == null
                || !await TryUpdateModelAsync(viewModel)
                || !partViewModel.PartDefinition.Fields.Any(p => p.Name == viewModel.Name))
                return NotFound();

            _contentDefinitionService.RemoveFieldFromPart(viewModel.Name, partViewModel.Name);

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                viewModel.Part = partViewModel;
                return View(viewModel);
            }

            _notifier.Success(T["The \"{0}\" field has been removed.", viewModel.Name]);

            if (_contentDefinitionService.GetType(id) != null)
                return RedirectToAction("Edit", new { id });

            return RedirectToAction("EditPart", new { id });
        }

        #endregion

    }
}
