using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class TenantHelper
{
    public static async Task VisitTenantSetupPageAsync(this IPage page, TenantInfo tenant)
    {
        await page.GotoAsync("/Admin/Tenants");
        await page.Locator($"#btn-setup-{tenant.Name}").ClickAsync();
    }

    public static async Task SiteSetupAsync(this IPage page, TenantInfo tenant)
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
            else if (!string.IsNullOrEmpty(tenant.TablePrefix))
            {
                // When using a shared database (non-SQLite), set the table prefix to isolate data.
                var tablePrefix = page.Locator("#TablePrefix");
                if (await tablePrefix.CountAsync() > 0 && await tablePrefix.IsVisibleAsync())
                {
                    await tablePrefix.FillAsync(tenant.TablePrefix);
                }
            }
        }

        await page.Locator("#UserName").FillAsync(config.Username);
        await page.Locator("#Email").FillAsync(config.Email);
        await page.Locator("#Password").FillAsync(config.Password);
        await page.Locator("#PasswordConfirmation").FillAsync(config.Password);
        await page.Locator("#SubmitButton").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public static async Task CreateTenantAsync(this IPage page, TenantInfo tenant)
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

        // Set database provider to Sqlite if not already set.
        // When DatabaseConfigurationPreset is true (external DB configured on the parent),
        // the #DatabaseProvider dropdown is not rendered — only the table prefix is shown.
        var dbProvider = page.Locator("#DatabaseProvider");
        if (await dbProvider.CountAsync() > 0)
        {
            var currentValue = await dbProvider.InputValueAsync();
            if (string.IsNullOrEmpty(currentValue))
            {
                await dbProvider.SelectOptionAsync("Sqlite");
            }
        }

        // Always set the table prefix when available, regardless of whether the provider dropdown is visible.
        var tablePrefix = page.Locator("#TablePrefix");
        if (await tablePrefix.CountAsync() > 0 && await tablePrefix.IsVisibleAsync())
        {
            var tablePrefixValue = string.IsNullOrEmpty(tenant.TablePrefix) ? tenant.Name : tenant.TablePrefix;
            await tablePrefix.FillAsync(tablePrefixValue);
        }

        await page.Locator("button.create[type=\"submit\"]").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public static async Task NewTenantAsync(this IPage page, TenantInfo tenant)
    {
        await page.LoginAsync();
        await page.CreateTenantAsync(tenant);
        await page.VisitTenantSetupPageAsync(tenant);
        await page.SiteSetupAsync(tenant);
    }
}
