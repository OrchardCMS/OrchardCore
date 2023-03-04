using OrchardCore.DisplayManagement;
using OrchardCore.Testing.Mocks;
using OrchardCore.Tests.Workflows.Activities;
using OrchardCore.Workflows.Activities;
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
            var scriptEvaluator = OrchardCoreMock.CreateWorkflowScriptEvaluator(serviceProvider);
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

            var workflowManager = OrchardCoreMock.CreateWorkflowManager(serviceProvider, new IActivity[] { addTask, writeLineTask, setOutputTask }, workflowType);
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
    }
}
