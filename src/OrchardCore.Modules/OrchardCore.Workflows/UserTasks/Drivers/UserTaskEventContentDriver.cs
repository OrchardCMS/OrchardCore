using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Workflows;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.UserTasks.Activities;
using OrchardCore.Workflows.UserTasks.ViewModels;

namespace OrchardCore.Workflows.UserTasks.Drivers
{
    public class UserTaskEventContentDriver : ContentDisplayDriver
    {
        private readonly IWorkflowStore _workflowStore;
        private readonly IActivityLibrary _activityLibrary;
        private readonly IWorkflowManager _workflowManager;
        private readonly INotifier _notifier;
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly IHtmlLocalizer H;

        public UserTaskEventContentDriver(
            IWorkflowStore workflowStore,
            IActivityLibrary activityLibrary,
            IWorkflowManager workflowManager,
            INotifier notifier,
            IHtmlLocalizer<UserTaskEventContentDriver> localizer,
            IHttpContextAccessor httpContextAccessor)
        {
            _workflowStore = workflowStore;
            _activityLibrary = activityLibrary;
            _workflowManager = workflowManager;
            _notifier = notifier;
            _httpContextAccessor = httpContextAccessor;

            H = localizer;
        }

        public override IDisplayResult Edit(ContentItem contentItem)
        {
            var results = new List<IDisplayResult>
            {
                Initialize<UserTaskEventContentViewModel>("Content_UserTaskButton", async model => {
                    var actions = await GetUserTaskActionsAsync(contentItem.ContentItemId);
                    model.Actions = actions;
                }).Location("Actions:30"),
            };

            return Combine(results.ToArray());
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentItem model, IUpdateModel updater)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var action = (string)httpContext.Request.Form["submit.Save"] ?? httpContext.Request.Form["submit.Publish"];
            if (action?.StartsWith("user-task.", StringComparison.Ordinal) == true)
            {
                action = action["user-task.".Length..];

                var availableActions = await GetUserTaskActionsAsync(model.ContentItemId);

                if (!availableActions.Contains(action))
                {
                    await _notifier.ErrorAsync(H["Not authorized to trigger '{0}'.", action]);
                }
                else
                {
                    var contentEvent = new ContentEventContext()
                    {
                        Name = nameof(UserTaskEvent),
                        ContentType = model.ContentType,
                        ContentItemId = model.ContentItemId
                    };

                    var input = new Dictionary<string, object>
                    {
                        { ContentEventConstants.UserActionInputKey, action },
                        { ContentEventConstants.ContentItemInputKey, model },
                        { ContentEventConstants.ContentEventInputKey, contentEvent }
                    };

                    await _workflowManager.TriggerEventAsync(nameof(UserTaskEvent), input, correlationId: model.ContentItemId);
                }
            }

            return Edit(model);
        }

        private async Task<IList<string>> GetUserTaskActionsAsync(string contentItemId)
        {
            var workflows = await _workflowStore.ListByActivityNameAsync(nameof(UserTaskEvent), contentItemId);
            var user = _httpContextAccessor.HttpContext.User;
            var userRoles = user.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
            var actionsQuery =
                from workflow in workflows
                let workflowState = workflow.State.ToObject<WorkflowState>()
                from blockingActivity in workflow.BlockingActivities
                where blockingActivity.Name == nameof(UserTaskEvent)
                from action in GetUserTaskActions(workflowState, blockingActivity.ActivityId, userRoles)
                select action;

            return actionsQuery.Distinct().ToList();
        }

        private IEnumerable<string> GetUserTaskActions(WorkflowState workflowState, string activityId, IEnumerable<string> userRoles)
        {
            if (workflowState.ActivityStates.TryGetValue(activityId, out var activityState))
            {
                var activity = _activityLibrary.InstantiateActivity<UserTaskEvent>(nameof(UserTaskEvent), activityState);

                if (activity.Roles.Any() && !userRoles.Any(x => activity.Roles.Contains(x)))
                    yield break;

                foreach (var action in activity.Actions)
                {
                    yield return action;
                }
            }
        }
    }
}
