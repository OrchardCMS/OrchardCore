using System.Threading.Tasks;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Models;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Tests.UI.Tests
{
    public class SaaSTests : UITestBase
    {
        private const string TestTenantUrlPrefix = "test";
        private const string TestTenantDisplayName = "Test Tenant";

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

        [Theory, Chrome]
        public Task CreatingTenantShouldWork(Browser browser) =>
            ExecuteTestAfterSetupAsync(
                async context =>
                {
                    // Taken from https://github.com/Lombiq/UI-Testing-Toolbox/blob/6eb53a55c991f9f3764660791e649783973236d1/Lombiq.Tests.UI.Samples/Tests/TenantTests.cs
                    const string tenantAdminName = "tenantAdmin";
                    await context.SignInDirectlyAsync();

                    await context.CreateAndEnterTenantAsync(
                        TestTenantDisplayName,
                        TestTenantUrlPrefix,
                        "Blog.Tests",
                        new CreateTenant { UserName = tenantAdminName });

                    Assert.Equal(TestTenantDisplayName, context.Get(By.ClassName("navbar-brand")).Text);

                    await context.SignInDirectlyAsync(tenantAdminName);
                    Assert.Equal(tenantAdminName, await context.GetCurrentUserNameAsync());
                    Assert.StartsWith($"/{TestTenantUrlPrefix}", context.GetCurrentUri().AbsolutePath);
                },
                browser,
                configuration =>
                {
                    configuration.AccessibilityCheckingConfiguration.RunAccessibilityCheckingAssertionOnAllPageChanges = true;
                    configuration.AccessibilityCheckingConfiguration.AxeBuilderConfigurator += axeBuilder =>
                        AccessibilityCheckingConfiguration
                            .ConfigureWcag21aa(axeBuilder)
                            .DisableRules("color-contrast");

                    return Task.CompletedTask;
                });
    }
}
