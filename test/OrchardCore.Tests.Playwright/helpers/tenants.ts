import { type Page } from '@playwright/test';
import { defaultOrchardConfig, type TenantInfo } from './utils';
import { login } from './auth';

export async function visitTenantSetupPage(page: Page, tenant: TenantInfo): Promise<void> {
    await page.goto('/Admin/Tenants');
    await page.locator(`#btn-setup-${tenant.name}`).click();
}

export async function siteSetup(page: Page, tenant: TenantInfo): Promise<void> {
    const config = defaultOrchardConfig;
    await page.locator('#SiteName').fill(tenant.name);

    // Set recipe value directly (hidden input or select depending on context)
    const recipeName = page.locator('#RecipeName');
    if (await recipeName.count() > 0) {
        // Use evaluate like Cypress's .val() to set the value without requiring an option match
        await recipeName.evaluate((el: HTMLElement, val: string) => {
            (el as HTMLInputElement | HTMLSelectElement).value = val;
            el.dispatchEvent(new Event('change', { bubbles: true }));
        }, tenant.setupRecipe);
    }

    // Set database provider to Sqlite if not already set
    const dbProvider = page.locator('#DatabaseProvider');
    if (await dbProvider.count() > 0) {
        const currentValue = await dbProvider.inputValue();
        if (!currentValue) {
            await dbProvider.selectOption('Sqlite');
        }
    }

    await page.locator('#UserName').fill(config.username);
    await page.locator('#Email').fill(config.email);
    await page.locator('#Password').fill(config.password);
    await page.locator('#PasswordConfirmation').fill(config.password);
    await page.locator('#SubmitButton').click();
    await page.waitForLoadState('networkidle');
}

export async function createTenant(page: Page, tenant: TenantInfo): Promise<void> {
    await page.goto('/Admin/Tenants');
    await page.locator('.btn.create').first().click();
    await page.locator('#Name').fill(tenant.name);
    await page.locator('#Description').fill(`Recipe: ${tenant.setupRecipe}. ${tenant.description || ''}`);
    await page.locator('#RequestUrlPrefix').fill(tenant.prefix);

    // Select recipe if available in the dropdown, otherwise skip (will be set during setup)
    const recipeSelect = page.locator('#RecipeName');
    const hasOption = await recipeSelect.locator(`option[value="${tenant.setupRecipe}"]`).count();
    if (hasOption > 0) {
        await recipeSelect.selectOption(tenant.setupRecipe);
    }

    // Set database provider to Sqlite if not already set by environment variable
    const dbProvider = page.locator('#DatabaseProvider');
    const currentValue = await dbProvider.inputValue();
    if (!currentValue) {
        await dbProvider.selectOption('Sqlite');
    } else {
        // If a provider is set (via env var), set the table prefix to the tenant name
        await page.locator('#TablePrefix').fill(tenant.name);
    }

    await page.locator('button.create[type="submit"]').click();
    await page.waitForLoadState('networkidle');
}

export async function newTenant(page: Page, tenant: TenantInfo): Promise<void> {
    await login(page);
    await createTenant(page, tenant);
    await visitTenantSetupPage(page, tenant);
    await siteSetup(page, tenant);
}

export async function enableFeature(page: Page, prefix: string, featureId: string): Promise<void> {
    // Load the Features page to get a valid session and antiforgery token
    await page.goto(`${prefix}/Admin/Features`);
    await page.waitForLoadState('networkidle');

    // Extract the antiforgery token
    const token = await page.evaluate(() =>
        document.querySelector<HTMLInputElement>('input[name=__RequestVerificationToken]')?.value ?? ''
    );

    // POST directly to the Enable endpoint
    const enableUrl = `${prefix}/Admin/Features/${featureId}/Enable`;
    const response = await page.request.post(enableUrl, {
        form: { __RequestVerificationToken: token },
    });

    if (!response.ok()) {
        const body = await response.text();
        throw new Error(`Failed to enable feature ${featureId}: ${response.status()} ${response.statusText()} - ${body.substring(0, 500)}`);
    }

    // After enabling, the shell is rebuilt on the next request.
    // Make a request to the tenant to ensure the new shell is active.
    await page.goto(`${prefix}/Admin`);
    await page.waitForLoadState('networkidle');
}
