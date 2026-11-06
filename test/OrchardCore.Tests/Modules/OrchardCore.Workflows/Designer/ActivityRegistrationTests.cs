using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Drivers;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Options;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Designer;

public sealed class ActivityRegistrationTests
{
    [Fact]
    public void AddActivity_WithConfigure_SetsIconOnRegistration()
    {
        var services = new ServiceCollection();

        services.AddActivity<NotifyTask, NotifyTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-star");
        var options = services.BuildServiceProvider().GetRequiredService<IOptions<WorkflowOptions>>().Value;

        var registration = options.GetActivityRegistration(typeof(NotifyTask));
        Assert.Equal("fa-solid fa-star", registration.Icon);
        Assert.Contains(typeof(NotifyTaskDisplayDriver), registration.DriverTypes);
    }

    [Fact]
    public void AddActivity_WithoutConfigure_LeavesIconUnset()
    {
        var services = new ServiceCollection();

        services.AddActivity<NotifyTask, NotifyTaskDisplayDriver>();
        var options = services.BuildServiceProvider().GetRequiredService<IOptions<WorkflowOptions>>().Value;

        Assert.Null(options.GetActivityRegistration(typeof(NotifyTask)).Icon);
        Assert.Null(options.GetActivityRegistration(typeof(LogTask)));
    }

    [Fact]
    public void RegisterActivity_SecondDriverWithConfigure_KeepsDriversAndAppliesConfigure()
    {
        var options = new WorkflowOptions();

        options.RegisterActivity(typeof(NotifyTask), typeof(NotifyTaskDisplayDriver));
        options.RegisterActivity(typeof(NotifyTask), typeof(ActivityMetadataDisplayDriver), activity => activity.Icon = "fa-solid fa-bell");

        var registration = options.GetActivityRegistration(typeof(NotifyTask));
        Assert.Equal(2, registration.DriverTypes.Count);
        Assert.Equal("fa-solid fa-bell", registration.Icon);
    }
}
