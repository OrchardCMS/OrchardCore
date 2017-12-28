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
        private readonly IWorkflowManager _workflowManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisplayManager<IActivity> _activityDisplayManager;
        private readonly INotifier _notifier;

        private dynamic New { get; }
        private IStringLocalizer S { get; }
        private IHtmlLocalizer H { get; }

        public WorkflowDefinitionController
        (
            ISiteService siteService,
            ISession session,
            IActivityLibrary activityLibrary,
            IWorkflowManager workflowManager,
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
            _workflowManager = workflowManager;
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


        [HttpPost]
        public async Task<IActionResult> EditProperties(WorkflowDefinitionPropertiesViewModel viewModel, int? id)
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
                    Name = viewModel.Name?.Trim(),
                    IsEnabled = true
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

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var availableActivities = _activityLibrary.ListActivities();
            var workflowDefinitionRecord = await _session.GetAsync<WorkflowDefinitionRecord>(id);
            var workflowContext = _workflowManager.CreateWorkflowContext(workflowDefinitionRecord, new WorkflowInstanceRecord { DefinitionId = workflowDefinitionRecord.Id });
            var activityContexts = workflowDefinitionRecord.Activities.Select(x => _workflowManager.CreateActivityContext(x)).ToList();
            var activityThumbnailDisplayTasks = availableActivities.Select((x, i) => BuildActivityDisplay(x, i, id, "Thumbnail"));
            var activityDesignDisplayTasks = activityContexts.Select((x, i) => BuildActivityDisplay(x, i, id, "Design"));

            await Task.WhenAll(activityThumbnailDisplayTasks.Concat(activityDesignDisplayTasks));

            var activityThumbnailShapes = activityThumbnailDisplayTasks.Select(x => x.Result).ToList();
            var activityDesignShapes = activityDesignDisplayTasks.Select(x => x.Result).ToList();
            var activitiesDataQuery = activityContexts.Select(x => new
            {
                Id = x.ActivityRecord.Id,
                X = x.ActivityRecord.X,
                Y = x.ActivityRecord.Y,
                Name = x.ActivityRecord.Name,
                IsStart = x.ActivityRecord.IsStart,
                Outcomes = x.Activity.GetPossibleOutcomes(workflowContext, x).ToArray()
            });
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
                WorkflowDefinitionJson = JsonConvert.SerializeObject(workflowDefinitionData, Formatting.None, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                ActivityThumbnailShapes = activityThumbnailShapes,
                ActivityDesignShapes = activityDesignShapes,
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
            var currentActivities = workflowDefinitionRecord.Activities.ToDictionary(x => x.Id);
            var postedActivities = ((IEnumerable<dynamic>)state.activities).ToDictionary(x => (int)x.id);
            var removedActivityIdsQuery =
                from activityId in currentActivities.Keys
                where !postedActivities.ContainsKey(activityId)
                select activityId;
            var removedActivityIds = removedActivityIdsQuery.ToList();

            // Remove any orphans (activities deleted on the client).
            foreach (var activityId in removedActivityIds)
            {
                var activityToRemove = currentActivities[activityId];
                workflowDefinitionRecord.Activities.Remove(activityToRemove);
                currentActivities.Remove(activityId);
            }

            foreach (var activityState in state.activities)
            {
                var activity = currentActivities[(int)activityState.id];
                activity.X = activityState.x;
                activity.Y = activityState.y;
                activity.IsStart = activityState.isStart;
            }

            workflowDefinitionRecord.Transitions.Clear();
            foreach (var transitionState in state.transitions)
            {
                workflowDefinitionRecord.Transitions.Add(new TransitionRecord
                {
                    SourceActivityId = transitionState.sourceActivityId,
                    DestinationActivityId = transitionState.destinationActivityId,
                    SourceOutcomeName = transitionState.sourceOutcomeName
                });
            }

            _session.Save(workflowDefinitionRecord);
            await _session.CommitAsync();
            _notifier.Success(H["Workflow Definition has been saved."]);
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }

        [HttpPost]
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

        private async Task<dynamic> BuildActivityDisplay(IActivity activity, int index, int workflowDefinitionId, string displayType)
        {
            dynamic activityShape = await _activityDisplayManager.BuildDisplayAsync(activity, this, displayType);
            activityShape.Metadata.Type = $"Activity_{displayType}";
            activityShape.Activity = activity;
            activityShape.WorkflowDefinitionId = workflowDefinitionId;
            activityShape.Index = index;
            return activityShape;
        }

        private async Task<dynamic> BuildActivityDisplay(ActivityContext activityContext, int index, int workflowDefinitionId, string displayType)
        {
            dynamic activityShape = await _activityDisplayManager.BuildDisplayAsync(activityContext.Activity, this, displayType);
            activityShape.Metadata.Type = $"Activity_{displayType}";
            activityShape.Activity = activityContext.Activity;
            activityShape.ActivityRecord = activityContext.ActivityRecord;
            activityShape.WorkflowDefinitionId = workflowDefinitionId;
            activityShape.Index = index;
            return activityShape;
        }
    }
}
