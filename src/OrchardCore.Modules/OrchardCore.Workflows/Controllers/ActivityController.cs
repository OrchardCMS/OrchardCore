using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Settings;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;
using YesSql;

namespace OrchardCore.Workflows.Controllers
{
    [Admin]
    public class ActivityController : Controller, IUpdateModel
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

        public ActivityController
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

        public async Task<IActionResult> Create(string activityName, int workflowDefinitionId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflowDefinition = await _session.GetAsync<WorkflowDefinitionRecord>(workflowDefinitionId);
            var activity = _activityLibrary.InstantiateActivity(activityName);
            var activityEditor = await _activityDisplayManager.BuildEditorAsync(activity, this, isNew: true);

            activityEditor.Metadata.Type = "Activity_Edit";

            var viewModel = new ActivityEditViewModel
            {
                Activity = activity,
                ActivityId = null,
                ActivityEditor = activityEditor,
                WorkflowDefinitionId = workflowDefinitionId
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string activityName, int workflowDefinitionId, ActivityEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflowDefinition = await _session.GetAsync<WorkflowDefinitionRecord>(workflowDefinitionId);
            var activity = _activityLibrary.InstantiateActivity(activityName);
            var activityEditor = await _activityDisplayManager.UpdateEditorAsync(activity, this, isNew: true);

            if (!ModelState.IsValid)
            {
                activityEditor.Metadata.Type = "Activity_Edit";
                model.Activity = activity;
                model.ActivityEditor = activityEditor;
                model.WorkflowDefinitionId = workflowDefinitionId;
                return View(model);
            }

            workflowDefinition.Activities.Add(new ActivityRecord
            {
                Name = activity.Name,
                Properties = activity.Properties,
                Id = workflowDefinition.Activities.Select(x => x.Id).OrderByDescending(x => x).FirstOrDefault() + 1
            });

            _session.Save(workflowDefinition);
            _notifier.Success(H["Activity added successfully"]);

            return RedirectToAction("Edit", "WorkflowDefinition", new { id = workflowDefinitionId });
        }

        public async Task<IActionResult> Edit(int workflowDefinitionId, int activityId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflowDefinition = await _session.GetAsync<WorkflowDefinitionRecord>(workflowDefinitionId);
            var activityRecord = workflowDefinition.Activities.Single(x => x.Id == activityId);
            var activityContext = _workflowManager.CreateActivityContext(activityRecord);
            var activityEditor = await _activityDisplayManager.BuildEditorAsync(activityContext.Activity, this, isNew: false);

            activityEditor.Metadata.Type = "Activity_Edit";

            var viewModel = new ActivityEditViewModel
            {
                Activity = activityContext.Activity,
                ActivityId = activityId,
                ActivityEditor = activityEditor,
                WorkflowDefinitionId = workflowDefinitionId
            };

            // To prevent a duplicate resource name from being generated (conflicting with Activity.Edit.cshtml),
            // we need to use a different view name.
            return View("Update", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ActivityEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflowDefinition = await _session.GetAsync<WorkflowDefinitionRecord>(model.WorkflowDefinitionId);
            var activityRecord = workflowDefinition.Activities.Single(x => x.Id == model.ActivityId.Value);
            var activityContext = _workflowManager.CreateActivityContext(activityRecord);
            var activityEditor = await _activityDisplayManager.UpdateEditorAsync(activityContext.Activity, this, isNew: false);

            if (!ModelState.IsValid)
            {
                activityEditor.Metadata.Type = "Activity_Edit";
                model.Activity = activityContext.Activity;
                model.ActivityEditor = activityEditor;
                model.WorkflowDefinitionId = model.WorkflowDefinitionId;

                // To prevent a duplicate resource name from being generated (conflicting with Activity.Edit.cshtml),
                // we need to use a different view name.
                return View("Update", model);
            }

            activityRecord.Properties = activityContext.Activity.Properties;

            _session.Save(workflowDefinition);
            _notifier.Success(H["Activity updated successfully"]);

            return RedirectToAction("Edit", "WorkflowDefinition", new { id = model.WorkflowDefinitionId });
        }
    }
}
