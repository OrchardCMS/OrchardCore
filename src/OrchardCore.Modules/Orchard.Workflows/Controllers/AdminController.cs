using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Notify;
using Orchard.DisplayManagement.Views;
using Orchard.Mvc.ActionConstraints;
using Orchard.Navigation;
using Orchard.Settings;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using Orchard.Workflows.ViewModels;
using YesSql;

namespace Orchard.Workflows.Controllers
{
    public class AdminController : Controller, IUpdateModel {
        private readonly ISiteService _siteService;
        private readonly ISession _session;
        private readonly IActivitiesManager _activitiesManager;
        private readonly IAuthorizationService _authorizationService;

        private readonly dynamic New;
        private readonly INotifier _notifier;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;

        public AdminController(
            ISiteService siteService,
            ISession session,
            IActivitiesManager activitiesManager,
            IAuthorizationService authorizationService,
            IShapeFactory shapeFactory,
            INotifier notifier,
            IStringLocalizer<AdminController> s,
            IHtmlLocalizer<AdminController> h
            ) {
            _siteService = siteService;
            _session = session;
            _activitiesManager = activitiesManager;
            _authorizationService = authorizationService;

            New = shapeFactory;
            _notifier = notifier;
            S = s;
            H = h;
        }

        public async Task<IActionResult> Index(AdminIndexOptions options, PagerParameters pagerParameters) {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            // default options
            if (options == null)
            {
                options = new AdminIndexOptions();
            }

            var workflows = _session.Query<WorkflowDefinition, WorkflowDefinitionIndex>();

            switch (options.Filter) {
                case WorkflowDefinitionFilter.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                workflows = workflows.Where(w => w.Name.Contains(options.Search));
            }

            switch (options.Order) {
                case WorkflowDefinitionOrder.Name:
                    workflows = workflows.OrderBy(u => u.Name);
                    break;
            }

            var count = await workflows.CountAsync();

            var results = await workflows
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ListAsync();

            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            var pagerShape = New.Pager(pager).TotalItemCount(count).RouteData(routeData);

            var model = new AdminIndexViewModel {
                WorkflowDefinitions = results
                    .Select(x => new WorkflowDefinitionEntry {
                        Definition = x,
                        DefinitionId = x.Id,
                        Name = x.Name})
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.BulkEdit")]
        public async Task<IActionResult> BulkEdit(AdminIndexOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var viewModel = new AdminIndexViewModel { WorkflowDefinitions = new List<WorkflowDefinitionEntry>(), Options = new AdminIndexOptions() };

            if (!(await TryUpdateModelAsync(viewModel)))
            {
                return View(viewModel);
            }

            var checkedEntries = viewModel.WorkflowDefinitions.Where(t => t.IsChecked);
            switch (options.BulkAction)
            {
                case WorkflowDefinitionBulk.None:
                    break;
                case WorkflowDefinitionBulk.Delete:
                    if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
                    {
                        return Unauthorized();
                    }

                    foreach (var entry in checkedEntries)
                    {
                        var workflowDefinition = await _session.GetAsync<WorkflowDefinition>(entry.DefinitionId);

                        if (workflowDefinition != null)
                        {
                            _session.Delete(workflowDefinition);
                            _notifier.Success(H["Workflow {0} deleted", workflowDefinition.Name]);
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return RedirectToAction("Index", new { page = pagerParameters.Page, pageSize = pagerParameters.PageSize });
        }

        public async Task<IActionResult> List(int id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflowDefinition = await _session.GetAsync<WorkflowDefinition>(id);

            if (workflowDefinition == null)
            {
                return NotFound();
            }

            var workflows = workflowDefinition.Workflows;

            var viewModel = New.ViewModel(
                Definition: workflowDefinition,
                Workflows: workflows
                );

            return View(viewModel);
        }

        public async Task<IActionResult> EditProperties(int id = 0)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            if (id == 0)
            {
                return View();
            }
            else
            {
                var workflowDefinition = await _session.GetAsync<WorkflowDefinition>(id);

                return View(new AdminEditViewModel { WorkflowDefinition = new WorkflowDefinitionViewModel { Name = workflowDefinition.Name, Id = workflowDefinition.Id } });
            }
        }


        [HttpPost, ActionName("EditProperties")]
        public async Task<IActionResult> EditPropertiesPost(AdminEditViewModel adminEditViewModel, int id = 0)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(adminEditViewModel.WorkflowDefinition.Name))
            {
                ModelState.AddModelError("Name", S["The Name can't be empty."]);
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            if (id == 0)
            {
                var workflowDefinition = new WorkflowDefinition
                {
                    Name = adminEditViewModel.WorkflowDefinition.Name
                };

                _session.Save(workflowDefinition);

                return RedirectToAction("Edit", new { workflowDefinition.Id });
            }
            else
            {
                var workflowDefinition = await _session.GetAsync<WorkflowDefinition>(id);

                workflowDefinition.Name = adminEditViewModel.WorkflowDefinition.Name;

                return RedirectToAction("Index");
            }
        }

        public async Task<JsonResult> State(int? id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                throw new AuthenticationException("");
            }

            var workflowDefinitionRecord = id.HasValue ? await _session.GetAsync<WorkflowDefinition>(id.Value) : null;
            var isRunning = workflowDefinitionRecord != null && workflowDefinitionRecord.Workflows.Any();
            return Json(new { isRunning = isRunning });
        }

        public async Task<IActionResult> Edit(int id, string localId, int? workflowId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            // convert the workflow definition into its view model
            var workflowDefinition = await _session.GetAsync<WorkflowDefinition>(id);
            var workflowDefinitionViewModel = CreateWorkflowDefinitionViewModel(workflowDefinition);
            var workflow = workflowId.HasValue ? await _session.GetAsync<Workflow>(workflowId.Value) : null;

            var viewModel = new AdminEditViewModel
            {
                LocalId = string.IsNullOrEmpty(localId) ? Guid.NewGuid().ToString() : localId,
                IsLocal = !string.IsNullOrEmpty(localId),
                WorkflowDefinition = workflowDefinitionViewModel,
                AllActivities = _activitiesManager.GetActivities(),
                Workflow = workflow
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflowDefinition = await _session.GetAsync<WorkflowDefinition>(id);

            if (workflowDefinition != null)
            {
                _session.Delete(workflowDefinition);
                _notifier.Success(H["Workflow definition {0} deleted", workflowDefinition.Name]);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWorkflow(int id, string returnUrl)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflow = await _session.GetAsync<Workflow>(id);

            if (workflow != null)
            {
                _session.Delete(workflow);
                _notifier.Success(H["Workflow deleted"]);
            }
            
            return RedirectToLocal(returnUrl);
        }

        private WorkflowDefinitionViewModel CreateWorkflowDefinitionViewModel(WorkflowDefinition workflowDefinition)
        {
            if (workflowDefinition == null)
            {
                throw new ArgumentNullException(nameof(workflowDefinition));
            }

            var workflowDefinitionModel = new WorkflowDefinitionViewModel
            {
                Id = workflowDefinition.Id,
                Name = workflowDefinition.Name
            };

            dynamic workflow = new JObject();
            workflow.Activities = new JArray(workflowDefinition.Activities.Select(x =>
            {
                dynamic activity = new JObject();
                activity.Name = x.Name;
                activity.Id = x.Id;
                activity.ClientId = x.GetClientId();
                activity.Left = x.X;
                activity.Top = x.Y;
                activity.Start = x.Start;
                //activity.State = FormParametersHelper.FromJsonString(x.State);

                return activity;
            }));

            workflow.Connections = new JArray(workflowDefinition.Transitions.Select(x =>
            {
                dynamic connection = new JObject();
                connection.Id = x.Id;
                connection.SourceId = x.SourceActivity.Name + "_" + x.SourceActivity.Id;
                connection.TargetId = x.DestinationActivity.Name + "_" + x.DestinationActivity.Id;
                connection.SourceEndpoint = x.SourceEndpoint;
                return connection;
            }));

            //workflowDefinitionModel.JsonData = FormParametersHelper.ToJsonString(workflow);

            return workflowDefinitionModel;
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public async Task<IActionResult> EditPost(int id, string localId, string data, bool clearWorkflows)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflowDefinition = await _session.GetAsync<WorkflowDefinition>(id);

            if (workflowDefinition == null)
            {
                return NotFound();
            }

            workflowDefinition.Enabled = true;

            //var state = FormParametersHelper.FromJsonString(data);
            //var activitiesIndex = new Dictionary<string, Activity>();

            //workflowDefinition.Activities.Clear();

            //foreach (var activity in state.Activities)
            //{
            //    Activity internalActivity;

            //    workflowDefinition.Activities.Add(internalActivity = new Activity
            //    {
            //        Name = activity.Name,
            //        X = activity.Left,
            //        Y = activity.Top,
            //        Start = activity.Start,
            //        State = FormParametersHelper.ToJsonString(activity.State),
            //        Definition = workflowDefinition
            //    });

            //    activitiesIndex.Add((string)activity.ClientId, activity);
            //}

            //workflowDefinition.Transitions.Clear();

            //foreach (var connection in state.Connections)
            //{
            //    workflowDefinition.Transitions.Add(new Transition
            //    {
            //        SourceActivity = activitiesIndex[(string)connection.SourceId],
            //        DestinationActivity = activitiesIndex[(string)connection.TargetId],
            //        SourceEndpoint = connection.SourceEndpoint,
            //        Definition = workflowDefinition
            //    });
            //}

            //if (clearWorkflows)
            //{
            //    workflowDefinition.Workflows.Clear();
            //}
            //else
            //{
            //    foreach (var workflowRecord in workflowDefinition.Workflows)
            //    {
            //        // Update any awaiting activity records with the new activity record.
            //        foreach (var awaitingActivityRecord in workflowRecord.AwaitingActivities)
            //        {
            //            var clientId = awaitingActivityRecord.Activity.GetClientId();
            //            if (activitiesIndex.ContainsKey(clientId))
            //            {
            //                awaitingActivityRecord.Activity = activitiesIndex[clientId];
            //            }
            //            else
            //            {
            //                workflowRecord.AwaitingActivities.Remove(awaitingActivityRecord);
            //            }
            //        }
            //        // Remove any workflows with no awaiting activities.
            //        if (!workflowRecord.AwaitingActivities.Any())
            //        {
            //            workflowDefinition.Workflows.Remove(workflowRecord);
            //        }
            //    }
            //}

            _notifier.Success(H["Workflow saved successfully"]);

            // Don't pass the localId to force the activites to refresh and use the deterministic clientId.
            return RedirectToAction("Edit", new { id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Cancel")]
        public async Task<IActionResult> EditPostCancel()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            return View();
        }

        //[Themed(false)]
        [HttpPost]
        public async Task<IActionResult> RenderActivity(ActivityViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var activity = _activitiesManager.GetActivityByName(model.Name);

            if (activity == null)
            {
                return NotFound();
            }

            dynamic shape = New.Activity(activity);

            //if (model.State != null)
            //{
            //    var state = FormParametersHelper.ToDynamic(FormParametersHelper.ToString(model.State));
            //    shape.State(state);
            //}
            //else
            //{
            //    shape.State(FormParametersHelper.FromJsonString("{}"));
            //}

            //shape.Metadata.Alternates.Add("Activity__" + activity.Name);

            //return new ShapeResult(this, shape);
            return null;
        }

        public async Task<IActionResult> EditActivity(string localId, string clientId, ActivityViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var activity = _activitiesManager.GetActivityByName(model.Name);

            if (activity == null)
            {
                return NotFound();
            }

            // build the form, and let external components alter it
            var form = "{ }";//activity.Form == null ? null : _formManager.Build(activity.Form);

            // form is bound on client side
            var viewModel = New.ViewModel(LocalId: localId, ClientId: clientId, Form: form);

            return View(viewModel);
        }

        [HttpPost, ActionName("EditActivity")]
        [FormValueRequired("_submit.Save")]
        public async Task<IActionResult> EditActivityPost(int id, string localId, string name, string clientId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var activity = _activitiesManager.GetActivityByName(name);

            if (activity == null)
            {
                return NotFound();
            }

            //// validating form values
            //_formManager.Validate(new ValidatingContext { FormName = activity.Form, ModelState = ModelState, ValueProvider = ValueProvider });

            //// stay on the page if there are validation errors
            //if (!ModelState.IsValid)
            //{

            //    // build the form, and let external components alter it
            //    var form = activity.Form == null ? null : _formManager.Build(activity.Form);

            //    // bind form with existing values.
            //    _formManager.Bind(form, ValueProvider);

            //    var viewModel = New.ViewModel(Id: id, LocalId: localId, Form: form);

            //    return View(viewModel);
            //}

            //var model = new UpdatedActivityModel
            //{
            //    ClientId = clientId,
            //    Data = JsonConvert.SerializeObject(
            //        FormParametersHelper.ToJsonString(formValues),
            //        new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml })
            //};

            //TempData["UpdatedViewModel"] = model;

            return RedirectToAction("Edit", new
            {
                id,
                localId
            });
        }

        [HttpPost, ActionName("EditActivity")]
        [FormValueRequired("_submit.Cancel")]
        public async Task<IActionResult> EditActivityPostCancel(int id, string localId, string name, string clientId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            return RedirectToAction("Edit", new { id, localId });
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("~/");
            }
        }
    }
}
