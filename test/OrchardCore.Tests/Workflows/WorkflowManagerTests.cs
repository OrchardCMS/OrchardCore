using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using OrchardCore.Scripting;
using OrchardCore.Tests.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using Xunit;

namespace OrchardCore.Tests.Workflows
{
    public class WorkflowManagerTests
    {
        [Fact]
        public async Task CanExecuteSimpleWorkflow()
        {
            var activityLibrary = new Mock<IActivityLibrary>();
            var workflowDefinitionRepository = new Mock<IWorkflowDefinitionRepository>();
            var workflowInstanceRepository = new Mock<IWorkflowInstanceRepository>();
            var logger = new Mock<ILogger<WorkflowManager>>();
            var localizer = new Mock<IStringLocalizer>();
            var scriptingManager = new Mock<IScriptingManager>();
            var workflowManager = new WorkflowManager(activityLibrary.Object, workflowDefinitionRepository.Object, workflowInstanceRepository.Object, scriptingManager.Object, logger.Object);
            var stringBuilder = new StringBuilder();
            var output = new StringWriter(stringBuilder);
            var addTask = new AddTask(localizer.Object);
            var writeLineTask = new WriteLineTask(localizer.Object, output);
            var activities = new IActivity[] { addTask, writeLineTask };
            var activityDictionary = activities.ToDictionary(x => x.Name);
            var workflowDefinition = new WorkflowDefinitionRecord
            {
                Id = 1,
                Activities = new List<ActivityRecord>
                {
                    new ActivityRecord { Id = 1, IsStart = true, Name = addTask.Name, Properties = JObject.FromObject( new
                    {
                        A = new WorkflowExpression<double>{ Expression = "js: WorkflowContext.Parameters.Get(\"A\")" },
                        B = new WorkflowExpression<double>{ Expression = "js: WorkflowContext.Parameters.Get(\"B\")" },
                    }) },
                    new ActivityRecord { Id = 2, Name = writeLineTask.Name, Properties = JObject.FromObject( new { Text = new WorkflowExpression<string>{ Expression = "js: WorkflowContext.Stack.Pop().ToString()" } }) }
                },
                Transitions = new List<TransitionRecord>
                {
                    new TransitionRecord{ SourceActivityId = 1, SourceOutcomeName = "Done", DestinationActivityId = 2 }
                }
            };

            foreach (var activity in activities)
            {
                activityLibrary.Setup(x => x.InstantiateActivity(activity.Name)).Returns(activity);
            }

            var a = 10d;
            var b = 22d;
            var expectedResult = a + b;

            workflowDefinitionRepository.Setup(x => x.GetWorkflowDefinitionAsync(workflowDefinition.Id)).Returns(Task.FromResult(workflowDefinition));
            scriptingManager.Setup(x => x.Evaluate("js: WorkflowContext.Parameters.Get(\"A\")")).Returns(a);
            scriptingManager.Setup(x => x.Evaluate("js: WorkflowContext.Parameters.Get(\"B\")")).Returns(b);
            scriptingManager.Setup(x => x.Evaluate("js: WorkflowContext.Stack.Pop().ToString()")).Returns(expectedResult.ToString());

            var workflowContext = await workflowManager.StartWorkflowAsync(workflowDefinition, new { A = a, B = b });
            var actualResult = (double)workflowContext.Stack.Peek();

            Assert.Equal(expectedResult, actualResult);
        }
    }
}
