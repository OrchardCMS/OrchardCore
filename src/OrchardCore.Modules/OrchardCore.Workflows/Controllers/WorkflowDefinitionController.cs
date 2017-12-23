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
using Newtonsoft.Json.Serialization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.ActionConstraints;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;
using YesSql;

namespace OrchardCore.Workflows.Controllers
{
    [Admin]
    public class WorkflowDefinitionController : Controller, IUpdateModel
    {
        private readonly ISiteService _siteService;
        private readonly ISession _session;
        private readonly IActivityLibrary _activityLibrary;
        private readonly IAuthorizationService _authorizationService;
        private IDisplayManager<IActivity> _activityDisplayManager;
        private readonly INotifier _notifier;

        private dynamic New { get; }
        private IStringLocalizer S { get; }
        private IHtmlLocalizer H { get; }

        public WorkflowDefinitionController
        (
            ISiteService siteService,
            ISession session,
            IActivityLibrary activityLibrary,
            IAuthorizationService authorizationService,
            IDisplayManager<IActivity> activityDisplayManager,
            IShapeFactory shapeFactory,
            INotifier notifier,
            IStringLocalizer<WorkflowDefinitionController> s,
            IHtmlLocalizer<WorkflowDefinitionController> h
        )
        {
            _siteService = siteService;
            _session = session;
            _activityLibrary = activityLibrary;
            _authorizationService = authorizationService;
            _activityDisplayManager = activityDisplayManager;
            _notifier = notifier;

            New = shapeFactory;
            S = s;
            H = h;
        }

        public async Task<IActionResult> Index(AdminIndexOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            if (options == null)
            {
                options = new AdminIndexOptions();
            }

            var query = _session.Query<WorkflowDefinitionRecord, WorkflowDefinitionByNameIndex>();

            switch (options.Filter)
            {
                case WorkflowDefinitionFilter.All:
                default:
                    break;
            }

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                query = query.Where(w => w.Name.Contains(options.Search));
            }

            switch (options.Order)
            {
                case WorkflowDefinitionOrder.Name:
                    query = query.OrderBy(u => u.Name);
                    break;
            }

            var count = await query.CountAsync();

            var results = await query
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ListAsync();

            // Maintain previous route data when generating page links.
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);

            var model = new AdminIndexViewModel
            {
                WorkflowDefinitions = results
                    .Select(x => new WorkflowDefinitionEntry
                    {
                        Definition = x,
                        DefinitionId = x.Id,
                        Name = x.Name
                    })
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            return View(model);
        }

        [HttpPost, ActionName(nameof(Index))]
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
                        var workflowDefinition = await _session.GetAsync<WorkflowDefinitionRecord>(entry.DefinitionId);

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

        public async Task<IActionResult> EditProperties(int? id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            if (id == null)
            {
                return View();
            }
            else
            {
                var workflowDefinition = await _session.GetAsync<WorkflowDefinitionRecord>(id.Value);

                return View(new WorkflowDefinitionPropertiesViewModel { Name = workflowDefinition.Name, Id = workflowDefinition.Id });
            }
        }


        [HttpPost, ActionName(nameof(EditProperties))]
        public async Task<IActionResult> EditPropertiesPost(WorkflowDefinitionPropertiesViewModel viewModel, int? id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(viewModel.Name))
            {
                ModelState.AddModelError("Name", S["The name can't be empty."]);
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            if (id == null)
            {
                var workflowDefinition = new WorkflowDefinitionRecord
                {
                    Name = viewModel.Name?.Trim()
                };

                _session.Save(workflowDefinition);

                return RedirectToAction("Edit", new { workflowDefinition.Id });
            }
            else
            {
                var workflowDefinition = await _session.GetAsync<WorkflowDefinitionRecord>(id.Value);
                if (workflowDefinition == null)
                {
                    return NotFound();
                }

                workflowDefinition.Name = viewModel.Name?.Trim();
                _session.Save(workflowDefinition);

                return RedirectToAction("Index");
            }
        }

        public async Task<JsonResult> State(int? id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                throw new AuthenticationException("");
            }

            var workflowDefinition = id.HasValue ? await _session.GetAsync<WorkflowDefinitionRecord>(id.Value) : null;
            var isRunning = false;

            if (workflowDefinition != null)
            {
                isRunning = await _session
                    .Query<WorkflowInstanceRecord, WorkflowInstanceByAwaitingActivitiesIndex>(query => query.Id == workflowDefinition.Id)
                    .CountAsync() > 0;
            }
            return Json(new { isRunning = isRunning });
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflowDefinitionRecord = await _session.GetAsync<WorkflowDefinitionRecord>(id);
            var activities = _activityLibrary.CreateActivities();
            var activityThumbnailDisplayTasks = activities.Select(async x =>
            {
                dynamic thumbnailShape = await _activityDisplayManager.BuildDisplayAsync(x, this, "Thumbnail");
                thumbnailShape.Metadata.Type = "Activity_Thumbnail";
                thumbnailShape.Activity = x;
                thumbnailShape.WorkflowDefinitionId = id;
                return thumbnailShape;
            });

            await Task.WhenAll(activityThumbnailDisplayTasks);
            var activityThumbnails = activityThumbnailDisplayTasks.Select(x => x.Result).ToList();
            var workflowContext = new WorkflowContext(workflowDefinitionRecord, new WorkflowInstanceRecord { DefinitionId = workflowDefinitionRecord.Id }, activities);
            var activitiesDataQuery =
                from activityDefinition in workflowDefinitionRecord.Activities
                let activity = workflowContext.GetActivityByName(activityDefinition.Name)
                let activityContext = new ActivityContext { Record = activityDefinition }
                select new
                {
                    Id = activityDefinition.Id,
                    X = activityDefinition.X,
                    Y = activityDefinition.Y,
                    Name = activityDefinition.Name,
                    IsStart = activityDefinition.IsStart,
                    Outcomes = activity.GetPossibleOutcomes(workflowContext, activityContext).ToArray()
                };
            var workflowDefinitionData = new
            {
                Id = workflowDefinitionRecord.Id,
                Name = workflowDefinitionRecord.Name,
                IsEnabled = workflowDefinitionRecord.IsEnabled,
                Activities = activitiesDataQuery.ToArray(),
                Transitions = workflowDefinitionRecord.Transitions
            };
            var viewModel = new WorkflowDefinitionViewModel
            {
                WorkflowDefinition = workflowDefinitionRecord,
                ActivityThumbnails = activityThumbnails,
                WorkflowEditor = await New.WorkflowEditor(
                    WorkflowDefinition: workflowDefinitionRecord,
                    WorkflowDefinitionData: JsonConvert.SerializeObject(workflowDefinitionData, Formatting.None, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() })
                )
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(WorkflowDefinitionUpdateModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflowDefinitionRecord = await _session.GetAsync<WorkflowDefinitionRecord>(model.Id);
            dynamic state = JObject.Parse(model.State);

            _notifier.Success(H["Workflow Definition has been saved."]);
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflowDefinition = await _session.GetAsync<WorkflowDefinitionRecord>(id);

            if (workflowDefinition != null)
            {
                _session.Delete(workflowDefinition);
                _notifier.Success(H["Workflow definition {0} deleted", workflowDefinition.Name]);
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName(nameof(Edit))]
        [FormValueRequired("submit.Save")]
        public async Task<IActionResult> EditPost(int id, string localId, string data, bool clearWorkflows)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflowDefinition = await _session.GetAsync<WorkflowDefinitionRecord>(id);

            if (workflowDefinition == null)
            {
                return NotFound();
            }

            workflowDefinition.IsEnabled = true;

            var activitiesIndex = new Dictionary<string, ActivityRecord>();

            workflowDefinition.Activities.Clear();

            //foreach (var activity in state.Activities)
            //{
            //    Activity internalActivity;

            //    workflowDefinition.Activities.Add(internalActivity = new Activity
            //    {
            //        Name = activity.Name,
            //        X = activity.Left,
            //        Y = activity.Top,
            //        Start = activity.Start,
            //        State = FormParametersHelper.ToJsonString(activity.State)
            //    });

            //    activitiesIndex.Add((string)activity.ClientId, internalActivity);
            //}

            workflowDefinition.Transitions.Clear();

            //foreach (var connection in state.Connections)
            //{
            //    workflowDefinition.Transitions.Add(new Transition
            //    {
            //        SourceActivityId = (string)connection.SourceId,
            //        DestinationActivityId = (string)connection.TargetId,
            //        SourceEndpoint = connection.SourceEndpoint
            //    });
            //}

            _session.Save(workflowDefinition);

            //var workflows = await _session
            //        .Query<Workflow, WorkflowWorkflowDefinitionIndex>(query => query.DefinitionId == workflowDefinition.Id)
            //        .ListAsync();

            //if (clearWorkflows)
            //{
            //    foreach (var workflow in workflows)
            //        _session.Delete(workflow);
            //}
            //else
            //{
            //    foreach (var workflow in workflows)
            //    {
            //        // Update any awaiting activity records with the new activity record.
            //        foreach (var awaitingActivity in workflow.AwaitingActivities)
            //        {
            //            var clientId = awaitingActivity.ClientId;
            //            if (activitiesIndex.ContainsKey(clientId))
            //            {
            //                awaitingActivity = activitiesIndex[clientId];
            //            }
            //            else
            //            {
            //                workflow.AwaitingActivities.Remove(awaitingActivityRecord);
            //            }
            //        }
            //        // Remove any workflows with no awaiting activities.
            //        if (workflow.AwaitingActivities.Count == 0)
            //        {
            //            _session.Delete(workflow);
            //        }
            //        else
            //        {
            //            _session.Save(workflow);
            //        }
            //    }
            //}

            _notifier.Success(H["Workflow saved successfully."]);

            // Don't pass the localId to force the activities to refresh and use the deterministic clientId.
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

        [HttpPost]
        public async Task<IActionResult> RenderActivity(ActivityViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var activity = (Activity)_activityLibrary.GetActivityByName(model.Name);

            if (activity == null)
            {
                return NotFound();
            }

            dynamic shape = await _activityDisplayManager.BuildDisplayAsync(activity, this, "Design");

            if (model.State != null)
            {
                //var state = FormParametersHelper.ToDynamic(FormParametersHelper.ToString(model.State));
                //shape.State(state);
            }
            else
            {
                //shape.State(FormParametersHelper.FromJsonString("{}"));
            }

            var viewModel = await New.ViewModel(Name: model.Name, EditorShape: shape);

            return View("RenderActivity", viewModel);
        }

        public async Task<IActionResult> EditActivity(string localId, string clientId, ActivityViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var activity = _activityLibrary.CreateActivity(model.Name);

            if (activity == null)
            {
                return NotFound();
            }

            dynamic shape = await New.Activity(activity);

            if (model.State != null)
            {
                //var state = FormParametersHelper.ToDynamic(FormParametersHelper.ToString(model.State));
                //shape.State(state);
            }
            else
            {
                //shape.State(FormParametersHelper.FromJsonString("{}"));
            }

            shape.Name = model.Name;

            // Form is bound on client side.
            var viewModel = await New.ViewModel(LocalId: localId, ClientId: clientId, EditorShape: shape);

            return View(viewModel);
        }

        [HttpPost, ActionName(nameof(EditActivity))]
        [FormValueRequired("_submit.Save")]
        public async Task<IActionResult> EditActivityPost(int id, string localId, string name, string clientId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var activity = _activityLibrary.CreateActivity(name);

            if (activity == null)
            {
                return NotFound();
            }

            //// validating form values
            //_formManager.Validate(new ValidatingContext { FormName = activity.Form, ModelState = ModelState, ValueProvider = ValueProvider });

            // stay on the page if there are validation errors
            if (!ModelState.IsValid)
            {

                // build the form, and let external components alter it

                var viewModel = await New.ViewModel(Id: id, LocalId: localId, Form: null);

                return View(viewModel);
            }

            var model = new UpdatedActivityModel
            {
                ClientId = clientId,
                //Data = FormParametersHelper.ToJsonString(Request.Form)
            };

            TempData["UpdatedViewModel"] = JsonConvert.SerializeObject(model);

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
