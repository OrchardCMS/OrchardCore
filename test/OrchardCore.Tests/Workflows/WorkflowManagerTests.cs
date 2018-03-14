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
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement;
using OrchardCore.Liquid;
using OrchardCore.Liquid.Services;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;
using OrchardCore.Tests.Workflows.Activities;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Evaluators;
using OrchardCore.Workflows.Expressions;
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
            var localizer = new Mock<IStringLocalizer>();

            var stringBuilder = new StringBuilder();
            var output = new StringWriter(stringBuilder);
            var addTask = new AddTask(localizer.Object);
            var writeLineTask = new WriteLineTask(localizer.Object, output);
            var workflowDefinition = new WorkflowDefinition
            {
                Id = 1,
                Activities = new List<ActivityRecord>
                {
                    new ActivityRecord { ActivityId = "1", IsStart = true, Name = addTask.Name, Properties = JObject.FromObject( new
                    {
                        A = new WorkflowExpression<double>("input(\"A\")"),
                        B = new WorkflowExpression<double>("input(\"B\")"),
                    }) },
                    new ActivityRecord { ActivityId = "2", Name = writeLineTask.Name, Properties = JObject.FromObject( new { Text = new WorkflowExpression<string>("lastResult().toString()") }) }
                },
                Transitions = new List<Transition>
                {
                    new Transition{ SourceActivityId = "1", SourceOutcomeName = "Done", DestinationActivityId = "2" }
                }
            };

            var workflowManager = CreateWorkflowManager(new IActivity[] { addTask, writeLineTask }, workflowDefinition);
            var a = 10d;
            var b = 22d;
            var expectedResult = (a + b).ToString() + System.Environment.NewLine;

            var workflowContext = await workflowManager.StartWorkflowAsync(workflowDefinition, input: new RouteValueDictionary(new { A = a, B = b }));
            var actualResult = stringBuilder.ToString();

            Assert.Equal(expectedResult, actualResult);
        }

        private WorkflowManager CreateWorkflowManager(IEnumerable<IActivity> activities, WorkflowDefinition workflowDefinition)
        {
            var services = new ServiceCollection();
            services.AddScoped(typeof(Resolver<>));
            services.AddScoped(provider => new Mock<IShapeFactory>().Object);
            services.AddScoped(provider => new Mock<IViewLocalizer>().Object);
            services.AddScoped<IWorkflowExecutionContextHandler, DefaultWorkflowExecutionContextHandler>(); // Necessary for tests involving script expressions.

            var serviceProvider = services.BuildServiceProvider();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var javaScriptEngine = new JavaScriptEngine(memoryCache, new Mock<IStringLocalizer<JavaScriptEngine>>().Object);
            var workflowContextHandlers = new Resolver<IEnumerable<IWorkflowExecutionContextHandler>>(serviceProvider);
            var workflowValueSerializers = new Resolver<IEnumerable<IWorkflowValueSerializer>>(serviceProvider);
            var globalMethodProviders = new IGlobalMethodProvider[0];
            var scriptingManager = new DefaultScriptingManager(new[] { javaScriptEngine }, globalMethodProviders, serviceProvider);
            var scriptEvaluator = new JavaScriptWorkflowScriptEvaluator(serviceProvider, scriptingManager, workflowContextHandlers.Resolve(), new Mock<IStringLocalizer<JavaScriptWorkflowScriptEvaluator>>().Object, new Mock<ILogger<JavaScriptWorkflowScriptEvaluator>>().Object);
            var liquidOptions = new Mock<IOptions<LiquidOptions>>();
            liquidOptions.SetupGet(x => x.Value).Returns(() => new LiquidOptions());
            var liquidTemplateManager = new LiquidTemplateManager(memoryCache, liquidOptions.Object, serviceProvider);
            var liquidEvaluator = new LiquidWorkflowExpressionEvaluator(serviceProvider, liquidTemplateManager, new Mock<IStringLocalizer<LiquidWorkflowExpressionEvaluator>>().Object, workflowContextHandlers.Resolve(), new Mock<ILogger<LiquidWorkflowExpressionEvaluator>>().Object);
            var activityLibrary = new Mock<IActivityLibrary>();
            var workflowDefinitionStore = new Mock<IWorkflowDefinitionStore>();
            var workflowInstanceStore = new Mock<IWorkflowInstanceStore>();
            var workflowInstanceIdGenerator = new Mock<IWorkflowInstanceIdGenerator>();
            var workflowManagerLogger = new Mock<ILogger<WorkflowManager>>();
            var workflowContextLogger = new Mock<ILogger<WorkflowExecutionContext>>();
            var missingActivityLogger = new Mock<ILogger<MissingActivity>>();
            var missingActivityLocalizer = new Mock<IStringLocalizer<MissingActivity>>();
            var clock = new Mock<IClock>();
            var workflowManager = new WorkflowManager(
                serviceProvider,
                activityLibrary.Object,
                workflowDefinitionStore.Object,
                workflowInstanceStore.Object,
                workflowInstanceIdGenerator.Object,
                liquidEvaluator,
                scriptEvaluator,
                workflowContextHandlers,
                workflowValueSerializers,
                workflowManagerLogger.Object,
                workflowContextLogger.Object,
                missingActivityLogger.Object,
                missingActivityLocalizer.Object,
                clock.Object);

            foreach (var activity in activities)
            {
                activityLibrary.Setup(x => x.InstantiateActivity(activity.Name)).Returns(activity);
            }

            workflowDefinitionStore.Setup(x => x.GetAsync(workflowDefinition.Id)).Returns(Task.FromResult(workflowDefinition));

            return workflowManager;
        }
    }
}
