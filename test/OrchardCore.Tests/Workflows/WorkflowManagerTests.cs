using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;
using OrchardCore.Tests.Workflows.Activities;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Evaluators;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.WorkflowContextProviders;
using Xunit;

namespace OrchardCore.Tests.Workflows
{
    public class WorkflowManagerTests
    {
        [Fact]
        public async Task CanExecuteSimpleWorkflow()
        {
            var serviceProvider = CreateServiceProvider();
            var scriptEvaluator = CreateWorkflowScriptEvaluator(serviceProvider);
            var localizer = new Mock<IStringLocalizer<AddTask>>();

            var stringBuilder = new StringBuilder();
            var output = new StringWriter(stringBuilder);
            var addTask = new AddTask(scriptEvaluator, localizer.Object);
            var writeLineTask = new WriteLineTask(scriptEvaluator, localizer.Object, output);
            var setOutputTask = new SetOutputTask(scriptEvaluator, new Mock<IStringLocalizer<SetOutputTask>>().Object);
            var workflowType = new WorkflowType
            {
                Id = 1,
                Activities = new List<ActivityRecord>
                {
                    new ActivityRecord { ActivityId = "1", IsStart = true, Name = addTask.Name, Properties = JObject.FromObject( new
                    {
                        A = new WorkflowExpression<double>("input(\"A\")"),
                        B = new WorkflowExpression<double>("input(\"B\")"),
                    }) },
                    new ActivityRecord { ActivityId = "2", Name = writeLineTask.Name, Properties = JObject.FromObject( new { Text = new WorkflowExpression<string>("lastResult().toString()") }) },
                    new ActivityRecord { ActivityId = "3", Name = setOutputTask.Name, Properties = JObject.FromObject( new { Value = new WorkflowExpression<string>("lastResult()"), OutputName = "Sum" }) }
                },
                Transitions = new List<Transition>
                {
                    new Transition{ SourceActivityId = "1", SourceOutcomeName = "Done", DestinationActivityId = "2" },
                    new Transition{ SourceActivityId = "2", SourceOutcomeName = "Done", DestinationActivityId = "3" }
                }
            };

            var workflowManager = CreateWorkflowManager(serviceProvider, new IActivity[] { addTask, writeLineTask, setOutputTask }, workflowType);
            var a = 10d;
            var b = 22d;
            var expectedSum = a + b;
            var expectedResult = expectedSum.ToString() + System.Environment.NewLine;

            var workflowExecutionContext = await workflowManager.StartWorkflowAsync(workflowType, input: new RouteValueDictionary(new { A = a, B = b }));
            var actualResult = stringBuilder.ToString();

            Assert.Equal(expectedResult, actualResult);
            Assert.True(workflowExecutionContext.Output.ContainsKey("Sum"));
            Assert.Equal(expectedSum, (double)workflowExecutionContext.Output["Sum"]);
        }

        private IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddScoped(typeof(Resolver<>));
            services.AddScoped(provider => new Mock<IShapeFactory>().Object);
            services.AddScoped(provider => new Mock<IViewLocalizer>().Object);
            services.AddScoped<IWorkflowExecutionContextHandler, DefaultWorkflowExecutionContextHandler>();

            return services.BuildServiceProvider();
        }

        private IWorkflowScriptEvaluator CreateWorkflowScriptEvaluator(IServiceProvider serviceProvider)
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var javaScriptEngine = new JavaScriptEngine(memoryCache);
            var workflowContextHandlers = new Resolver<IEnumerable<IWorkflowExecutionContextHandler>>(serviceProvider);
            var globalMethodProviders = new IGlobalMethodProvider[0];
            var scriptingManager = new DefaultScriptingManager(new[] { javaScriptEngine }, globalMethodProviders);

            return new JavaScriptWorkflowScriptEvaluator(
                scriptingManager,
                workflowContextHandlers.Resolve(),
                new Mock<ILogger<JavaScriptWorkflowScriptEvaluator>>().Object
            );
        }

        private WorkflowManager CreateWorkflowManager(
            IServiceProvider serviceProvider,
            IEnumerable<IActivity> activities,
            WorkflowType workflowType
        )
        {
            var workflowValueSerializers = new Resolver<IEnumerable<IWorkflowValueSerializer>>(serviceProvider);
            var activityLibrary = new Mock<IActivityLibrary>();
            var workflowTypeStore = new Mock<IWorkflowTypeStore>();
            var workflowStore = new Mock<IWorkflowStore>();
            var workflowIdGenerator = new Mock<IWorkflowIdGenerator>();
            var workflowManagerLogger = new Mock<ILogger<WorkflowManager>>();
            var workflowContextLogger = new Mock<ILogger<WorkflowExecutionContext>>();
            var missingActivityLogger = new Mock<ILogger<MissingActivity>>();
            var missingActivityLocalizer = new Mock<IStringLocalizer<MissingActivity>>();
            var clock = new Mock<IClock>();
            var workflowManager = new WorkflowManager(
                activityLibrary.Object,
                workflowTypeStore.Object,
                workflowStore.Object,
                workflowIdGenerator.Object,
                workflowValueSerializers,
                workflowManagerLogger.Object,
                missingActivityLogger.Object,
                missingActivityLocalizer.Object,
                clock.Object);

            foreach (var activity in activities)
            {
                activityLibrary.Setup(x => x.InstantiateActivity(activity.Name)).Returns(activity);
            }

            workflowTypeStore.Setup(x => x.GetAsync(workflowType.Id)).Returns(Task.FromResult(workflowType));

            return workflowManager;
        }
    }
}
