using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Options;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Designer;

public sealed class ActivityProvidedValuesTests
{
    [Fact]
    public void GetProvidedValues_RegistrationAndActivity_AreMergedAndTheActivityWins()
    {
        var options = new WorkflowOptions();
        options.RegisterActivity(typeof(ProvidingTask), null, registration => registration
            .Provides(WorkflowValueSource.Input, "Owner", "string", "From the registration.")
            .Provides(WorkflowValueSource.Properties, "Status"));

        var values = new ProvidingTask().GetProvidedValues(options);

        Assert.Equal(
            [("Properties", "Status", "any", ""), ("Input", "Owner", "contentItem", "From the activity."), ("Output", "Result", "any", "")],
            values.Select(value => (value.Source.ToString(), value.Name, value.TypeName, value.Description?.Value ?? string.Empty)));
    }

    [Fact]
    public void Provides_Members_DeclaresTheFieldsOfTheValue()
    {
        var registration = new ActivityRegistration(typeof(ProvidingTask))
            .Provides(WorkflowValueSource.Input, "Customer", "object", "The customer.", [new ActivityProvidedValueMember { Name = "Email", TypeName = "string" }])
            .Provides(WorkflowValueSource.Input, "Order");

        Assert.Equal(["Email"], registration.ProvidedValues[0].Members.Select(member => member.Name));
        Assert.Empty(registration.ProvidedValues[1].Members);
    }

    [Fact]
    public void GetProvidedValues_ActivityWithoutDeclarations_ReturnsNone()
    {
        Assert.Empty(new ProvidingTask(declare: false).GetProvidedValues(new WorkflowOptions()));
    }

    private sealed class ProvidingTask : TaskActivity<ProvidingTask>, IActivityProvidedValues
    {
        private readonly bool _declare;

        public ProvidingTask(bool declare = true)
        {
            _declare = declare;
        }

        public override LocalizedString DisplayText => new(Name, Name);

        public override LocalizedString Category => new("Test", "Test");

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => Outcome(new LocalizedString("Done", "Done"));

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => Outcome("Done");

        public IEnumerable<ActivityProvidedValue> GetProvidedValues()
            => _declare
                ? [
                    new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "Owner", TypeName = "contentItem", Description = new("Owner", "From the activity.") },
                    new ActivityProvidedValue { Source = WorkflowValueSource.Output, Name = "Result" },
                ]
                : [];
    }
}
