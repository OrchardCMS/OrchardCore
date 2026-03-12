import { test, expect } from '@playwright/test';
import { generateTenantInfo, type TenantInfo } from '../../helpers/utils';
import { newTenant } from '../../helpers/tenants';
import { login } from '../../helpers/auth';

test.describe('Headless Recipe test', () => {
    let tenant: TenantInfo;

    test.beforeAll(async ({ browser }) => {
        tenant = generateTenantInfo('Headless');
        const page = await browser.newPage();
        await newTenant(page, tenant);
        await page.close();
    });

    test('Displays the login screen when accessing the root of the Headless theme', async ({ page }) => {
        await page.goto(`/${tenant.prefix}`);
        await expect(page.locator('#LoginForm_UserName')).toBeVisible();
    });

    test('Headless admin login should work', async ({ page }) => {
        await login(page, { prefix: `/${tenant.prefix}` });
        await page.goto(`/${tenant.prefix}/Admin`);
        await expect(page.locator('.menu-admin')).toHaveAttribute('id', 'adminMenu');
    });
});
