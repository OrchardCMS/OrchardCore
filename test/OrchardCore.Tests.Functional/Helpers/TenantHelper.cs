using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class TenantHelper
{
    public static async Task VisitTenantSetupPageAsync(IPage page, TenantInfo tenant)
    {
        await page.GotoAsync("/Admin/Tenants");
        await page.Locator($"#btn-setup-{tenant.Name}").ClickAsync();
    }

    public static async Task SiteSetupAsync(IPage page, TenantInfo tenant)
    {
        var config = TestUtils.DefaultConfig;
        await page.Locator("#SiteName").FillAsync(tenant.Name);

        // Set recipe value directly.
        var recipeName = page.Locator("#RecipeName");
        if (await recipeName.CountAsync() > 0)
        {
            await recipeName.EvaluateAsync(
                "(el, val) => { el.value = val; el.dispatchEvent(new Event('change', { bubbles: true })); }",
                tenant.SetupRecipe);
        }

        // Set database provider to Sqlite if not already set.
        var dbProvider = page.Locator("#DatabaseProvider");
        if (await dbProvider.CountAsync() > 0)
        {
            var currentValue = await dbProvider.InputValueAsync();
            if (string.IsNullOrEmpty(currentValue))
            {
                await dbProvider.SelectOptionAsync("Sqlite");
            }
        }

        await page.Locator("#UserName").FillAsync(config.Username);
        await page.Locator("#Email").FillAsync(config.Email);
        await page.Locator("#Password").FillAsync(config.Password);
        await page.Locator("#PasswordConfirmation").FillAsync(config.Password);
        await page.Locator("#SubmitButton").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public static async Task CreateTenantAsync(IPage page, TenantInfo tenant)
    {
        await page.GotoAsync("/Admin/Tenants");
        await page.Locator(".btn.create").First.ClickAsync();
        await page.Locator("#Name").FillAsync(tenant.Name);
        await page.Locator("#Description").FillAsync($"Recipe: {tenant.SetupRecipe}. {tenant.Description}");
        await page.Locator("#RequestUrlPrefix").FillAsync(tenant.Prefix);

        // Select recipe if available in the dropdown, otherwise skip.
        var recipeSelect = page.Locator("#RecipeName");
        var hasOption = await recipeSelect.Locator($"option[value=\"{tenant.SetupRecipe}\"]").CountAsync();
        if (hasOption > 0)
        {
            await recipeSelect.SelectOptionAsync(tenant.SetupRecipe);
        }

        // Set database provider to Sqlite if not already set by environment variable.
        var dbProvider = page.Locator("#DatabaseProvider");
        var currentValue = await dbProvider.InputValueAsync();
        if (string.IsNullOrEmpty(currentValue))
        {
            await dbProvider.SelectOptionAsync("Sqlite");
        }
        else
        {
            // If a provider is set (via env var), set the table prefix to the tenant name, if the field exists.
            var tablePrefix = page.Locator("#TablePrefix");
            if (await tablePrefix.CountAsync() > 0)
            {
                await tablePrefix.FillAsync(tenant.Name);
            }
        }

        await page.Locator("button.create[type=\"submit\"]").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public static async Task NewTenantAsync(IPage page, TenantInfo tenant)
    {
        await AuthHelper.LoginAsync(page);
        await CreateTenantAsync(page, tenant);
        await VisitTenantSetupPageAsync(page, tenant);
        await SiteSetupAsync(page, tenant);
    }
}
