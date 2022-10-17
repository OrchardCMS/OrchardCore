using System;
using System.Threading.Tasks;
using Lombiq.Tests.UI;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Tests.UI.Tests
{
    public class MvcTests : OrchardCoreUITestBase<Program>
    {
        public MvcTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Theory, Chrome]
        public Task BasicOrchardFeaturesShouldWorkWithBlank(Browser browser) =>
            ExecuteTestAsync(
                async context =>
                {
                    await context.GoToRelativeUrlAsync("/");
                    Assert.Equal("Hello World", context.Get(By.TagName("h1")).Text);
                },
                browser,
                configuration =>
                {
                    configuration.HtmlValidationConfiguration.RunHtmlValidationAssertionOnAllPageChanges = false;
                    return Task.CompletedTask;
                });

        protected override Task ExecuteTestAfterSetupAsync(Func<UITestContext, Task> testAsync, Browser browser, Func<OrchardCoreUITestExecutorConfiguration, Task> changeConfigurationAsync)
        {
            throw new NotImplementedException();
        }
    }
}
