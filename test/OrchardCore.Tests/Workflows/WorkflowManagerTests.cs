using OrchardCore.DisplayManagement;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;
using OrchardCore.Tests.Workflows.Activities;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Evaluators;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.WorkflowContextProviders;

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
                WorkflowTypeId = IdGenerator.GenerateId(),
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

        private static IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddScoped(typeof(Resolver<>));
            services.AddScoped(provider => new Mock<IShapeFactory>().Object);
            services.AddScoped(provider => new Mock<IViewLocalizer>().Object);
            services.AddScoped<IWorkflowExecutionContextHandler, DefaultWorkflowExecutionContextHandler>();

            return services.BuildServiceProvider();
        }

        private static IWorkflowScriptEvaluator CreateWorkflowScriptEvaluator(IServiceProvider serviceProvider)
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var javaScriptEngine = new JavaScriptEngine(memoryCache);
            var workflowContextHandlers = new Resolver<IEnumerable<IWorkflowExecutionContextHandler>>(serviceProvider);
            var globalMethodProviders = Array.Empty<IGlobalMethodProvider>();
            var scriptingManager = new DefaultScriptingManager(new[] { javaScriptEngine }, globalMethodProviders);

            return new JavaScriptWorkflowScriptEvaluator(
                scriptingManager,
                workflowContextHandlers.Resolve(),
                new Mock<ILogger<JavaScriptWorkflowScriptEvaluator>>().Object
            );
        }

        private static WorkflowManager CreateWorkflowManager(
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
            workflowIdGenerator.Setup(x => x.GenerateUniqueId(It.IsAny<Workflow>())).Returns(IdGenerator.GenerateId());
            var distributedLock = new Mock<IDistributedLock>();
            var workflowManagerLogger = new Mock<ILogger<WorkflowManager>>();
            var workflowContextLogger = new Mock<ILogger<WorkflowExecutionContext>>();
            var missingActivityLogger = new Mock<ILogger<MissingActivity>>();
            var missingActivityLocalizer = new Mock<IStringLocalizer<MissingActivity>>();
            var clock = new Mock<IClock>();
            var workflowFaultHandler = new Mock<IWorkflowFaultHandler>();
            var workflowManager = new WorkflowManager(
                activityLibrary.Object,
                workflowTypeStore.Object,
                workflowStore.Object,
                workflowIdGenerator.Object,
                workflowValueSerializers,
                workflowFaultHandler.Object,
                distributedLock.Object,
                workflowManagerLogger.Object,
                missingActivityLogger.Object,
                missingActivityLocalizer.Object,
                clock.Object
                );

            foreach (var activity in activities)
            {
                activityLibrary.Setup(x => x.InstantiateActivity(activity.Name)).Returns(activity);
            }

            workflowTypeStore.Setup(x => x.GetAsync(workflowType.Id)).Returns(Task.FromResult(workflowType));

            return workflowManager;
        }
    }
}
