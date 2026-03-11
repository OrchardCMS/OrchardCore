import { type Page } from '@playwright/test';
import { defaultOrchardConfig, type TenantInfo } from './utils';
import { login } from './auth';
import { btnCreateClick } from './buttons';

export async function visitTenantSetupPage(page: Page, tenant: TenantInfo): Promise<void> {
    await page.goto('/Admin/Tenants');
    await page.locator(`#btn-setup-${tenant.name}`).click();
}

export async function siteSetup(page: Page, tenant: TenantInfo): Promise<void> {
    const config = defaultOrchardConfig;
    await page.locator('#SiteName').fill(tenant.name);

    // Set recipe if the field exists
    const recipeName = page.locator('#RecipeName');
    if (await recipeName.count() > 0) {
        await recipeName.evaluate((el: HTMLInputElement, val: string) => { el.value = val; }, tenant.setupRecipe);
    }

    // Set database provider to Sqlite if not already set
    const dbProvider = page.locator('#DatabaseProvider');
    if (await dbProvider.count() > 0) {
        const currentValue = await dbProvider.inputValue();
        if (currentValue === '') {
            await dbProvider.evaluate((el: HTMLSelectElement) => { el.value = 'Sqlite'; });
        }
    }

    await page.locator('#UserName').fill(config.username);
    await page.locator('#Email').fill(config.email);
    await page.locator('#Password').fill(config.password);
    await page.locator('#PasswordConfirmation').fill(config.password);
    await page.locator('#SubmitButton').click();
}

export async function createTenant(page: Page, tenant: TenantInfo): Promise<void> {
    await page.goto('/Admin/Tenants');
    await btnCreateClick(page);
    await page.locator('#Name').fill(tenant.name);
    await page.locator('#Description').fill(`Recipe: ${tenant.setupRecipe}. ${tenant.description || ''}`);
    await page.locator('#RequestUrlPrefix').fill(tenant.prefix);
    await page.locator('#RecipeName').selectOption(tenant.setupRecipe);

    // Set database provider to Sqlite if not already set by environment variable
    const dbProvider = page.locator('#DatabaseProvider');
    if (await dbProvider.count() > 0) {
        const currentValue = await dbProvider.inputValue();
        if (currentValue === '') {
            await dbProvider.evaluate((el: HTMLSelectElement) => { el.value = 'Sqlite'; });
        } else {
            // If a provider is set (via env var), set the table prefix to the tenant name
            const tablePrefix = page.locator('#TablePrefix');
            if (await tablePrefix.count() > 0) {
                await tablePrefix.evaluate((el: HTMLInputElement, val: string) => { el.value = val; }, tenant.name);
            }
        }
    }

    await btnCreateClick(page);
}

export async function newTenant(page: Page, tenant: TenantInfo): Promise<void> {
    await login(page);
    await createTenant(page, tenant);
    await visitTenantSetupPage(page, tenant);
    await siteSetup(page, tenant);
}
