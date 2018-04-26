using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.UserTasks.Activities;
using OrchardCore.Workflows.UserTasks.ViewModels;

namespace OrchardCore.Workflows.UserTasks.Drivers
{
    public class ContentsDriver : ContentDisplayDriver
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IWorkflowStore _workflowStore;
        private readonly IActivityLibrary _activityLibrary;
        private readonly IWorkflowManager _workflowManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContentsDriver(
            IContentDefinitionManager contentDefinitionManager, 
            IWorkflowStore workflowStore,
            IActivityLibrary activityLibrary,
            IWorkflowManager workflowManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _workflowStore = workflowStore;
            _activityLibrary = activityLibrary;
            _workflowManager = workflowManager;
            _httpContextAccessor = httpContextAccessor;
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
            var action = (string)httpContext.Request.Form["submit.Save"];
            if (action?.StartsWith("user-task.") == true)
            {
                action = action.Substring("user-task.".Length);

                var input = new { UserAction = action };
                await _workflowManager.TriggerEventAsync(nameof(UserTaskEvent), input, model.ContentItemId);
            }

            return await EditAsync(model, updater);
        }
        
        private async Task<IList<string>> GetUserTaskActionsAsync(string contentItemId)
        {
            var workflows = await _workflowStore.ListAsync(nameof(UserTaskEvent), contentItemId);
            var actionsQuery =
                from workflow in workflows
                let workflowState = workflow.State.ToObject<WorkflowState>()
                from blockingActivity in workflow.BlockingActivities
                where blockingActivity.Name == nameof(UserTaskEvent)
                from action in GetUserTaskActions(workflowState, blockingActivity.ActivityId)
                select action;

            return actionsQuery.Distinct().ToList();
        }

        private IEnumerable<string> GetUserTaskActions(WorkflowState workflowState, string activityId)
        {
            if(workflowState.ActivityStates.TryGetValue(activityId, out var activityState))
            {
                var activity = _activityLibrary.InstantiateActivity<UserTaskEvent>(nameof(UserTaskEvent), activityState);

                foreach (var action in activity.Actions)
                {
                    yield return action;
                }
            }
        }
    }
}