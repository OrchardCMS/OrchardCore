using System.Threading.Tasks;
using Atata;
using Lombiq.Tests.UI.Attributes;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Xunit;
using Xunit.Abstractions;

namespace OrchardCore.Tests.UI.Tests
{
    public class AgencyRecipeTests : UITestBase
    {
        public AgencyRecipeTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Theory, Chrome]
        public Task BasicOrchardFeaturesShouldWorkWithAgency(Browser browser) =>
            ExecuteTestAsync(
                async context =>
                {
                    await context.TestSetupWithInvalidAndValidDataAsync(new OrchardCoreSetupParameters(context)
                    {
                        RecipeId = "Agency.Tests",
                    });

                    await context.TestRegistrationWithInvalidDataAsync();
                    await context.TestRegistrationAsync();
                    await context.TestRegistrationWithAlreadyRegisteredEmailAsync();
                    await context.TestLoginWithInvalidDataAsync();
                    await context.TestLoginAsync();

                    // This is similar to BasicOrchardFeaturesTestingUITestContextExtensions.TestContentOperationsAsync(),
                    // but the original method can only check page titles that are inside an <h1> HTML tag. Here the page
                    // title is inside a <div> HTML tag.
                    var pageTitle = "Test page";

                    var contentItemsPage = await context.GoToContentItemsPageAsync();
                    context.RefreshCurrentAtataContext();
                    contentItemsPage
                        .CreateNewPage()
                            .Title.Set(pageTitle)
                            .Publish.ClickAndGo()
                        .AlertMessages.Should.Contain(message => message.IsSuccess)
                        .Items[item => item.Title == pageTitle].View.Click();

                    var page = new OrdinaryPage(pageTitle);
                    context.Scope.AtataContext.Go.ToNextWindow(page)
                        .AggregateAssert(page => page
                        .PageTitle.Should.Contain(pageTitle));

                    context.Driver.Exists(By.XPath($"//div[contains(text(), '" + pageTitle + "')]").Visible());

                    page.CloseWindow();

                    await context.TestTurningFeatureOnAndOffAsync();
                    await context.TestLogoutAsync();
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
