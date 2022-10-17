using System.Threading.Tasks;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Tests.UI.Tests
{
    public class SaaSTests : UITestBase
    {
        public SaaSTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Theory, Chrome]
        public Task BasicOrchardFeaturesShouldWorkWithSaaS(Browser browser) =>
            ExecuteTestAsync(
                async context =>
                {
                    await context.TestSetupWithInvalidAndValidDataAsync(new OrchardCoreSetupParameters(context)
                    {
                        RecipeId = "SaaS.Tests",
                    });

                    await context.TestRegistrationWithInvalidDataAsync();
                    await context.TestRegistrationAsync();
                    await context.TestRegistrationWithAlreadyRegisteredEmailAsync();
                    await context.TestLoginWithInvalidDataAsync();
                    await context.TestLoginAsync();
                    await context.TestTurningFeatureOnAndOffAsync();
                    await context.TestLogoutAsync();
                },
                browser,
                configuration =>
                {
                    configuration.AccessibilityCheckingConfiguration.RunAccessibilityCheckingAssertionOnAllPageChanges = true;
                    return Task.CompletedTask;
                });
    }
}
