using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.Tokens.Services;
using Orchard.Workflows.Models;
using YesSql;

namespace Orchard.Workflows.Services
{
    public class WorkflowManager : IWorkflowManager
    {
        private readonly IActivitiesManager _activitiesManager;
        private readonly ISession _session;
        private readonly ITokenizer _tokenizer;

        private readonly ILogger L;

        public WorkflowManager(
            IActivitiesManager activitiesManager,
            ISession session,
            ITokenizer tokenizer,
            ILogger<WorkflowManager> logger)
        {
            _activitiesManager = activitiesManager;
            _session = session;
            _tokenizer = tokenizer;

            L = logger;
        }

        public async Task TriggerEvent(string name, IContent target, Func<Dictionary<string, object>> tokensContext)
        {
            var tokens = tokensContext();

            var activity = _activitiesManager.GetActivityByName(name);

            if (activity == null)
            {
                L.LogError("Activity {0} was not found", name);
                return;
            }

            // look for workflow definitions with a corresponding starting activity
            // it's important to return activities at this point and not workflows,
            // as a workflow definition could have multiple entry points with the same type of activity
            var startedActivities =
                (await _session
                    .Query<Activity, ActivityIndex>(index =>
                        index.Name == name &&
                        index.Start &&
                        index.DefinitionEnabled)
                    .ListAsync()).ToArray();

            // and any running workflow paused on this kind of activity for this content
            // it's important to return activities at this point as a workflow could be awaiting 
            // on several ones. When an activity is restarted, all the other ones of the same workflow are cancelled.
            var awaitingQuery = _session
                .Query<Activity, AwaitingActivityIndex>(index =>
                    index.ActivityName == name &&
                    index.ActivityStart == false);

            var awaitingActivities = (await awaitingQuery.ListAsync()).ToArray();

            // if no activity record is matching the event, do nothing
            if (startedActivities.Length == 0 && awaitingActivities.Length == 0)
            {
                return;
            }

            // resume halted workflows
            foreach (var awaitingActivity in awaitingActivities)
            {
                var workflow = await _session.GetAsync<Workflow>(awaitingActivity.WorkflowId);
                var workflowContext = new WorkflowContext
                {
                    Content = target,
                    Tokens = tokens,
                    Workflow = workflow
                };

                workflowContext.Tokens["Workflow"] = workflowContext;

                var activityContext = CreateActivityContext(awaitingActivity, tokens);

                // check the condition
                try
                {
                    if (!activity.CanExecute(workflowContext, activityContext))
                    {
                        continue;
                    }
                }
                catch (Exception e)
                {
                    L.LogError("Error while evaluating an activity condition on {0}: {1}", name, e.ToString());
                    continue;
                }

                await ResumeWorkflow(workflow, awaitingActivity, workflowContext, tokens);
            }

            // start new workflows
            foreach (var startedActivity in startedActivities)
            {
                var workflowContext = new WorkflowContext
                {
                    Content = target,
                    Tokens = tokens,
                };

                workflowContext.Tokens["Workflow"] = workflowContext;

                var definition = (await _session.GetAsync<Workflow>(startedActivity.WorkflowId)).Definition;

                var workflow = new Workflow
                {
                    Definition = definition,
                    State = "{}"
                };

                workflowContext.Workflow = workflow;

                var activityContext = CreateActivityContext(startedActivity, tokens);

                // check the condition
                try
                {
                    if (!activity.CanExecute(workflowContext, activityContext))
                    {
                        continue;
                    }
                }
                catch (Exception e)
                {
                    L.LogError("Error while evaluating an activity condition on {0}: {1}", name, e.ToString());
                    continue;
                }

                await StartWorkflow(workflowContext, startedActivity, tokens);
            }
        }

        private ActivityContext CreateActivityContext(Activity activity, IDictionary<string, object> tokens)
        {
            return new ActivityContext
            {
                Record = activity,
                Activity = _activitiesManager.GetActivityByName(activity.Name),
                State = new Lazy<dynamic>(() => GetState(activity.State, tokens))
            };
        }

        private async Task StartWorkflow(WorkflowContext workflowContext, Activity activity, IDictionary<string, object> tokens)
        {

            // signal every activity that the workflow is about to start
            var cancellationToken = new CancellationToken();
            InvokeActivities(a => a.OnWorkflowStarting(workflowContext, cancellationToken));

            if (cancellationToken.IsCancellationRequested)
            {
                // workflow is aborted
                return;
            }

            // signal every activity that the workflow is has started
            InvokeActivities(a => a.OnWorkflowStarted(workflowContext));

            var blockedOn = (await ExecuteWorkflow(workflowContext, activity, tokens)).ToList();

            // is the workflow halted on a blocking activity ?
            if (blockedOn.Count==0)
            {
                // no, nothing to do
            }
            else
            {
                // workflow halted, create a workflow state
                _session.Save(workflowContext.Workflow);

                foreach (var blocking in blockedOn)
                {
                    workflowContext.Workflow.AwaitingActivities.Add(blocking);
                }
            }
        }

        private async Task ResumeWorkflow(Workflow workflow, Activity awaitingActivity, WorkflowContext workflowContext, IDictionary<string, object> tokens)
        {
            // signal every activity that the workflow is about to be resumed
            var cancellationToken = new CancellationToken();
            InvokeActivities(a => a.OnWorkflowResuming(workflowContext, cancellationToken));

            if (cancellationToken.IsCancellationRequested)
            {
                // workflow is aborted
                return;
            }

            // signal every activity that the workflow is resumed
            InvokeActivities(activity => activity.OnWorkflowResumed(workflowContext));

            workflowContext.Workflow = workflow;

            workflow.AwaitingActivities.Remove(awaitingActivity);

            var blockedOn = (await ExecuteWorkflow(workflowContext, awaitingActivity, tokens)).ToList();

            // is the workflow halted on a blocking activity, and there is no more awaiting activities
            if (blockedOn.Count == 0 && workflow.AwaitingActivities.Count == 0)
            {
                // no, delete the workflow
                _session.Delete(workflow);
            }
            else
            {
                // add the new ones
                foreach (var blocking in blockedOn)
                {
                    workflow.AwaitingActivities.Add(blocking);
                }
            }
        }

        public async Task<IEnumerable<Activity>> ExecuteWorkflow(WorkflowContext workflowContext, Activity activity, IDictionary<string, object> tokens)
        {
            var firstPass = true;
            var scheduled = new Stack<Activity>();

            scheduled.Push(activity);

            var blocking = new List<Activity>();

            while (scheduled.Count > 0)
            {
                activity = scheduled.Pop();

                var activityContext = CreateActivityContext(activity, tokens);

                // while there is an activity to process

                if (!firstPass)
                {
                    if (activityContext.Activity.IsEvent)
                    {
                        blocking.Add(activity);
                        continue;
                    }
                }
                else
                {
                    firstPass = false;
                }

                // signal every activity that the activity is about to be executed
                var cancellationToken = new CancellationToken();
                InvokeActivities(a => a.OnActivityExecuting(workflowContext, activityContext, cancellationToken));

                if (cancellationToken.IsCancellationRequested)
                {
                    // activity is aborted
                    continue;
                }

                var outcomes = activityContext.Activity.Execute(workflowContext, activityContext).ToList();

                // signal every activity that the activity is executed
                InvokeActivities(a => a.OnActivityExecuted(workflowContext, activityContext));

                foreach (var outcome in outcomes)
                {
                    var definition = await _session.GetAsync<WorkflowDefinition>(workflowContext.Workflow.Definition.Id);
                    // look for next activity in the graph
                    var transition = definition.Transitions.FirstOrDefault(x => x.SourceActivity == activity && x.SourceEndpoint == outcome.Value);

                    if (transition != null)
                    {
                        scheduled.Push(transition.DestinationActivity);
                    }
                }
            }

            // apply Distinct() as two paths could block on the same activity
            return blocking.Distinct();
        }

        /// <summary>
        /// Executes a specific action on all the activities of a workflow, using a specific context
        /// </summary>
        private void InvokeActivities(Action<IActivity> action)
        {
            foreach (var activity in _activitiesManager.GetActivities())
            {
                action(activity);
            }
        }

        private dynamic GetState(string state, IDictionary<string, object> tokens) {
            if (!string.IsNullOrWhiteSpace(state)) {
                var formatted = JsonConvert.DeserializeXNode(state, "Root").ToString();
                var tokenized = _tokenizer.Tokenize(formatted, tokens);
                var serialized = string.IsNullOrEmpty(tokenized) ? "{}" : JsonConvert.SerializeXNode(XElement.Parse(tokenized));
                return FormParametersHelper.FromJsonString(serialized).Root;
            }

            return FormParametersHelper.FromJsonString("{}");
        }
    }
}
