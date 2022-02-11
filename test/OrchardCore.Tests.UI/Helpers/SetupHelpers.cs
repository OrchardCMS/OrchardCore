using System;
using System.Threading.Tasks;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Pages;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;

namespace OrchardCore.Tests.UI.Helpers
{
    public static class SetupHelpers
    {
        public const string RecipeId = "Blog";

        public static async Task<Uri> RunSetupAsync(UITestContext context)
        {
            var setupPage = await context.GoToSetupPageAsync();
            setupPage = await setupPage.SetupOrchardCoreAsync(
                context,
                new OrchardCoreSetupParameters(context)
                {
                    SiteName = "Orchard Core - UI Testing",
                    RecipeId = RecipeId,
                    TablePrefix = "oc",
                    SiteTimeZoneValue = "America/New_York",
                });

            context.Exists(By.Id("navbar"));

            return setupPage.PageUri.Value;
        }
    }
}
