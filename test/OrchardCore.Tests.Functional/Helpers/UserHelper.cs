using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class UserHelper
{
    /// <summary>
    /// Creates a new user via the Admin UI and optionally assigns a role.
    /// The caller must already be logged in as an admin on the target tenant.
    /// </summary>
    public static async Task CreateUserAsync(
        IPage page,
        string prefix,
        string userName,
        string email,
        string password,
        string roleName = null)
    {
        await page.GotoAsync($"{prefix}/Admin/Users/Create");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.Locator("input[name='UserInformation.UserName']").FillAsync(userName);
        await page.Locator("input[name='UserInformation.Email']").FillAsync(email);
        await page.Locator("input[name='UserFields.Password']").FillAsync(password);
        await page.Locator("input[name='UserFields.PasswordConfirmation']").FillAsync(password);

        if (!string.IsNullOrEmpty(roleName))
        {
            // Find the role checkbox by its associated label text.
            var roleLabel = page.Locator($"label.form-check-label:has-text('{roleName}')");
            await roleLabel.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
            await roleLabel.ClickAsync();
        }

        await ButtonHelper.ClickCreateAsync(page);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Logs in as a specific user (not the default admin).
    /// </summary>
    public static async Task LoginAsAsync(IPage page, string prefix, string userName, string password)
    {
        // Logout first if already logged in.
        await page.GotoAsync($"{prefix}/logout");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAsync($"{prefix}/login");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.Locator("#LoginForm_UserName").FillAsync(userName);
        await page.Locator("#LoginForm_Password").FillAsync(password);
        await page.Locator("button[type='submit']").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
