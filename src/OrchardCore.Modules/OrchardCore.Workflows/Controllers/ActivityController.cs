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
        private readonly IActivityIdGenerator _activityIdGenerator;
        private readonly IAuthorizationService _authorizationService;
        private readonly IActivityDisplayManager _activityDisplayManager;
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
            IActivityIdGenerator activityIdGenerator,
            IAuthorizationService authorizationService,
            IActivityDisplayManager activityDisplayManager,
            IShapeFactory shapeFactory,
            INotifier notifier,
            IStringLocalizer<ActivityController> s,
            IHtmlLocalizer<ActivityController> h
        )
        {
            _siteService = siteService;
            _session = session;
            _activityLibrary = activityLibrary;
            _workflowManager = workflowManager;
            _activityIdGenerator = activityIdGenerator;
            _authorizationService = authorizationService;
            _activityDisplayManager = activityDisplayManager;
            _notifier = notifier;

            New = shapeFactory;
            S = s;
            H = h;
        }

        public async Task<IActionResult> Create(string activityName, int workflowDefinitionId, string returnUrl)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var activity = _activityLibrary.InstantiateActivity(activityName);
            var workflowDefinition = await _session.GetAsync<WorkflowDefinition>(workflowDefinitionId);
            var activityId = _activityIdGenerator.GenerateUniqueId(new ActivityRecord());
            var activityEditor = await _activityDisplayManager.BuildEditorAsync(activity, this, isNew: true);

            activityEditor.Metadata.Type = "Activity_Edit";

            var viewModel = new ActivityEditViewModel
            {
                Activity = activity,
                ActivityId = activityId,
                ActivityEditor = activityEditor,
                WorkflowDefinitionId = workflowDefinitionId,
                ReturnUrl = returnUrl
            };

            if (!activity.HasEditor)
            {
                // No editor to show; short-circuit to the "POST" action.
                return await Create(activityName, viewModel);
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string activityName, ActivityEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflowDefinition = await _session.GetAsync<WorkflowDefinition>(model.WorkflowDefinitionId);
            var activity = _activityLibrary.InstantiateActivity(activityName);
            var activityEditor = await _activityDisplayManager.UpdateEditorAsync(activity, this, isNew: true);

            if (!ModelState.IsValid)
            {
                activityEditor.Metadata.Type = "Activity_Edit";
                model.Activity = activity;
                model.ActivityEditor = activityEditor;
                return View(model);
            }

            var activityRecord = new ActivityRecord
            {
                ActivityId = model.ActivityId,
                Name = activity.Name,
                Properties = activity.Properties,
            };
            workflowDefinition.Activities.Add(activityRecord);

            _session.Save(workflowDefinition);
            _notifier.Success(H["Activity added successfully"]);

            return Url.IsLocalUrl(model.ReturnUrl) ? (IActionResult)Redirect(model.ReturnUrl) : RedirectToAction("Edit", "WorkflowDefinition", new { id = model.WorkflowDefinitionId });
        }

        public async Task<IActionResult> Edit(int workflowDefinitionId, string activityId, string returnUrl)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflowDefinition = await _session.GetAsync<WorkflowDefinition>(workflowDefinitionId);
            var activityRecord = workflowDefinition.Activities.Single(x => x.ActivityId == activityId);
            var activityContext = await _workflowManager.CreateActivityExecutionContextAsync(activityRecord, activityRecord.Properties);
            var activityEditor = await _activityDisplayManager.BuildEditorAsync(activityContext.Activity, this, isNew: false);

            activityEditor.Metadata.Type = "Activity_Edit";

            var viewModel = new ActivityEditViewModel
            {
                Activity = activityContext.Activity,
                ActivityId = activityId,
                ActivityEditor = activityEditor,
                WorkflowDefinitionId = workflowDefinitionId,
                ReturnUrl = returnUrl
            };

            return View("EditActivity", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ActivityEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
            {
                return Unauthorized();
            }

            var workflowDefinition = await _session.GetAsync<WorkflowDefinition>(model.WorkflowDefinitionId);
            var activityRecord = workflowDefinition.Activities.Single(x => x.ActivityId == model.ActivityId);
            var activityContext = await _workflowManager.CreateActivityExecutionContextAsync(activityRecord, activityRecord.Properties);
            var activityEditor = await _activityDisplayManager.UpdateEditorAsync(activityContext.Activity, this, isNew: false);

            if (!ModelState.IsValid)
            {
                activityEditor.Metadata.Type = "Activity_Edit";
                model.Activity = activityContext.Activity;
                model.ActivityEditor = activityEditor;

                return View("EditActivity", model);
            }

            activityRecord.Properties = activityContext.Activity.Properties;

            _session.Save(workflowDefinition);
            _notifier.Success(H["Activity updated successfully"]);

            return Url.IsLocalUrl(model.ReturnUrl) 
                ? (IActionResult)Redirect(model.ReturnUrl) 
                : RedirectToAction("Edit", "WorkflowDefinition", new { id = model.WorkflowDefinitionId });
        }
    }
}
