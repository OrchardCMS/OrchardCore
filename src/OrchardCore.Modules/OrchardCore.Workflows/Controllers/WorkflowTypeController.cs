using System;
using System.Collections.Generic;
using System.Linq;
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
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Workflows.Controllers
{
    [Admin]
    public class WorkflowTypeController : Controller, IUpdateModel
    {
        private readonly ISiteService _siteService;
        private readonly ISession _session;
        private readonly IActivityLibrary _activityLibrary;
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowTypeStore _workflowDefinitionStore;
        private readonly IWorkflowTypeIdGenerator _workflowDefinitionIdGenerator;
        private readonly IAuthorizationService _authorizationService;
        private readonly IActivityDisplayManager _activityDisplayManager;
        private readonly INotifier _notifier;
        private readonly ISecurityTokenService _securityTokenService;

        private dynamic New { get; }
        private IStringLocalizer S { get; }
        private IHtmlLocalizer H { get; }

        public WorkflowTypeController
        (
            ISiteService siteService,
            ISession session,
            IActivityLibrary activityLibrary,
            IWorkflowManager workflowManager,
            IWorkflowTypeStore workflowDefinitionStore,
            IWorkflowTypeIdGenerator workflowDefinitionIdGenerator,
            IAuthorizationService authorizationService,
            IActivityDisplayManager activityDisplayManager,
            IShapeFactory shapeFactory,
            INotifier notifier,
            ISecurityTokenService securityTokenService,
            IStringLocalizer<WorkflowTypeController> s,
            IHtmlLocalizer<WorkflowTypeController> h
        )
        {
            _siteService = siteService;
            _session = session;
            _activityLibrary = activityLibrary;
            _workflowManager = workflowManager;
            _workflowDefinitionStore = workflowDefinitionStore;
            _workflowDefinitionIdGenerator = workflowDefinitionIdGenerator;
            _authorizationService = authorizationService;
            _activityDisplayManager = activityDisplayManager;
            _notifier = notifier;
            _securityTokenService = securityTokenService;

            New = shapeFactory;
            S = s;
            H = h;
        }

        public async Task<IActionResult> Index(WorkflowTypeIndexOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            if (options == null)
            {
                options = new WorkflowTypeIndexOptions();
            }

            var query = _session.Query<WorkflowType, WorkflowDefinitionIndex>();

            switch (options.Filter)
            {
                case WorkflowTypeFilter.All:
                default:
                    break;
            }

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                query = query.Where(w => w.Name.Contains(options.Search));
            }

            switch (options.Order)
            {
                case WorkflowTypeOrder.Name:
                    query = query.OrderBy(u => u.Name);
                    break;
            }

            var count = await query.CountAsync();

            var workflowDefinitions = await query
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ListAsync();

            var workflowDefinitionIds = workflowDefinitions.Select(x => x.WorkflowTypeId).ToList();
            var workflowInstanceGroups = (await _session.QueryIndex<WorkflowInstanceIndex>(x => x.WorkflowDefinitionId.IsIn(workflowDefinitionIds)).ListAsync()).GroupBy(x => x.WorkflowDefinitionId).ToDictionary(x => x.Key);

            // Maintain previous route data when generating page links.
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);
            var model = new WorkflowTypeIndexViewModel
            {
                WorkflowTypes = workflowDefinitions
                    .Select(x => new WorkflowTypeEntry
                    {
                        WorkflowType = x,
                        Id = x.Id,
                        WorkflowCount = workflowInstanceGroups.ContainsKey(x.WorkflowTypeId) ? workflowInstanceGroups[x.WorkflowTypeId].Count() : 0,
                        Name = x.Name
                    })
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            return View(model);
        }

        [HttpPost]
        [ActionName(nameof(Index))]
        [FormValueRequired("BulkAction")]
        public async Task<IActionResult> BulkEdit(WorkflowTypeBulkAction bulkAction, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var viewModel = new WorkflowTypeIndexViewModel { WorkflowTypes = new List<WorkflowTypeEntry>(), Options = new WorkflowTypeIndexOptions() };

            if (!(await TryUpdateModelAsync(viewModel)))
            {
                return View(viewModel);
            }

            var checkedEntries = viewModel.WorkflowTypes.Where(t => t.IsChecked);
            switch (bulkAction)
            {
                case WorkflowTypeBulkAction.None:
                    break;
                case WorkflowTypeBulkAction.Delete:
                    foreach (var entry in checkedEntries)
                    {
                        var workflowDefinition = await _workflowDefinitionStore.GetAsync(entry.Id);

                        if (workflowDefinition != null)
                        {
                            await _workflowDefinitionStore.DeleteAsync(workflowDefinition);
                            _notifier.Success(H["Workflow {0} has been deleted.", workflowDefinition.Name]);
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return RedirectToAction("Index", new { page = pagerParameters.Page, pageSize = pagerParameters.PageSize });
        }

        public async Task<IActionResult> EditProperties(int? id, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            if (id == null)
            {
                return View(new WorkflowDefinitionPropertiesViewModel
                {
                    IsEnabled = true,
                    ReturnUrl = returnUrl
                });
            }
            else
            {
                var workflowDefinition = await _session.GetAsync<WorkflowType>(id.Value);

                return View(new WorkflowDefinitionPropertiesViewModel
                {
                    Id = workflowDefinition.Id,
                    Name = workflowDefinition.Name,
                    IsEnabled = workflowDefinition.IsEnabled,
                    IsSingleton = workflowDefinition.IsSingleton,
                    DeleteFinishedWorkflows = workflowDefinition.DeleteFinishedWorkflows,
                    ReturnUrl = returnUrl
                });
            }
        }


        [HttpPost]
        public async Task<IActionResult> EditProperties(WorkflowDefinitionPropertiesViewModel viewModel, int? id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var isNew = id == null;
            var workflowDefinitionRecord = default(WorkflowType);

            if (isNew)
            {
                workflowDefinitionRecord = new WorkflowType();
                workflowDefinitionRecord.WorkflowTypeId = _workflowDefinitionIdGenerator.GenerateUniqueId(workflowDefinitionRecord);
            }
            else
            {
                workflowDefinitionRecord = await _session.GetAsync<WorkflowType>(id.Value);

                if (workflowDefinitionRecord == null)
                {
                    return NotFound();
                }
            }

            workflowDefinitionRecord.Name = viewModel.Name?.Trim();
            workflowDefinitionRecord.IsEnabled = viewModel.IsEnabled;
            workflowDefinitionRecord.IsSingleton = viewModel.IsSingleton;
            workflowDefinitionRecord.DeleteFinishedWorkflows = viewModel.DeleteFinishedWorkflows;

            await _workflowDefinitionStore.SaveAsync(workflowDefinitionRecord);

            return isNew
                ? RedirectToAction("Edit", new { workflowDefinitionRecord.Id })
                : Url.IsLocalUrl(viewModel.ReturnUrl)
                   ? (IActionResult)Redirect(viewModel.ReturnUrl)
                   : RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id, string localId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var newLocalId = string.IsNullOrWhiteSpace(localId) ? Guid.NewGuid().ToString() : localId;
            var availableActivities = _activityLibrary.ListActivities();
            var workflowDefinitionRecord = await _session.GetAsync<WorkflowType>(id);
            var workflowInstance = _workflowManager.NewWorkflowInstance(workflowDefinitionRecord);
            var workflowContext = await _workflowManager.CreateWorkflowExecutionContextAsync(workflowDefinitionRecord, workflowInstance);
            var activityContexts = await Task.WhenAll(workflowDefinitionRecord.Activities.Select(async x => await _workflowManager.CreateActivityExecutionContextAsync(x, x.Properties)));
            var activityThumbnailDisplayTasks = availableActivities.Select(async (x, i) => await BuildActivityDisplay(x, i, id, newLocalId, "Thumbnail"));
            var activityDesignDisplayTasks = activityContexts.Select(async (x, i) => await BuildActivityDisplay(x, i, id, newLocalId, "Design"));
            var workflowInstanceCount = await _session.QueryIndex<WorkflowInstanceIndex>(x => x.WorkflowDefinitionId == workflowDefinitionRecord.WorkflowTypeId).CountAsync();

            await Task.WhenAll(activityThumbnailDisplayTasks.Concat(activityDesignDisplayTasks));

            var activityThumbnailShapes = activityThumbnailDisplayTasks.Select(x => x.Result).ToList();
            var activityDesignShapes = activityDesignDisplayTasks.Select(x => x.Result).ToList();
            var activitiesDataQuery = activityContexts.Select(x => new
            {
                Id = x.ActivityRecord.ActivityId,
                X = x.ActivityRecord.X,
                Y = x.ActivityRecord.Y,
                Name = x.ActivityRecord.Name,
                IsStart = x.ActivityRecord.IsStart,
                IsEvent = x.Activity.IsEvent(),
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
                ActivityCategories = _activityLibrary.ListCategories().ToList(),
                LocalId = newLocalId,
                LoadLocalState = !string.IsNullOrWhiteSpace(localId),
                WorkflowInstanceCount = workflowInstanceCount
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

            var workflowDefinitionRecord = await _workflowDefinitionStore.GetAsync(model.Id);
            dynamic state = JObject.Parse(model.State);
            var currentActivities = workflowDefinitionRecord.Activities.ToDictionary(x => x.ActivityId);
            var postedActivities = ((IEnumerable<dynamic>)state.activities).ToDictionary(x => (string)x.id);
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

            // Update activities.
            foreach (var activityState in state.activities)
            {
                var activity = currentActivities[(string)activityState.id];
                activity.X = activityState.x;
                activity.Y = activityState.y;
                activity.IsStart = activityState.isStart;
            }

            // Update transitions.
            workflowDefinitionRecord.Transitions.Clear();
            foreach (var transitionState in state.transitions)
            {
                workflowDefinitionRecord.Transitions.Add(new Transition
                {
                    SourceActivityId = transitionState.sourceActivityId,
                    DestinationActivityId = transitionState.destinationActivityId,
                    SourceOutcomeName = transitionState.sourceOutcomeName
                });
            }

            await _workflowDefinitionStore.SaveAsync(workflowDefinitionRecord);
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

            var workflowDefinition = await _workflowDefinitionStore.GetAsync(id);

            if (workflowDefinition == null)
            {
                return NotFound();
            }

            await _workflowDefinitionStore.DeleteAsync(workflowDefinition);
            _notifier.Success(H["Workflow definition {0} deleted", workflowDefinition.Name]);

            return RedirectToAction("Index");
        }

        private async Task<dynamic> BuildActivityDisplay(IActivity activity, int index, int workflowDefinitionId, string localId, string displayType)
        {
            dynamic activityShape = await _activityDisplayManager.BuildDisplayAsync(activity, this, displayType);
            activityShape.Metadata.Type = $"Activity_{displayType}";
            activityShape.Activity = activity;
            activityShape.WorkflowDefinitionId = workflowDefinitionId;
            activityShape.Index = index;
            activityShape.ReturnUrl = Url.Action(nameof(Edit), new { id = workflowDefinitionId, localId = localId });
            return activityShape;
        }

        private async Task<dynamic> BuildActivityDisplay(ActivityContext activityContext, int index, int workflowDefinitionId, string localId, string displayType)
        {
            dynamic activityShape = await _activityDisplayManager.BuildDisplayAsync(activityContext.Activity, this, displayType);
            activityShape.Metadata.Type = $"Activity_{displayType}";
            activityShape.Activity = activityContext.Activity;
            activityShape.ActivityRecord = activityContext.ActivityRecord;
            activityShape.WorkflowDefinitionId = workflowDefinitionId;
            activityShape.Index = index;
            activityShape.ReturnUrl = Url.Action(nameof(Edit), new { id = workflowDefinitionId, localId = localId });
            return activityShape;
        }
    }
}
