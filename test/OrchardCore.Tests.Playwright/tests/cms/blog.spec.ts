import { test, expect } from '@playwright/test';
import { generateTenantInfo, type TenantInfo } from '../../helpers/utils';
import { newTenant } from '../../helpers/tenants';
import { login } from '../../helpers/auth';

test.describe('Blog Tests', () => {
    let tenant: TenantInfo;

    test.beforeAll(async ({ browser }) => {
        tenant = generateTenantInfo('Blog');
        const page = await browser.newPage();
        await newTenant(page, tenant);
        await page.close();
    });

    test('Displays the home page of the blog recipe', async ({ page }) => {
        await page.goto(`/${tenant.prefix}`);
        await expect(page.locator('.subheading')).toContainText('This is the description of your blog');
    });

    test('Blog admin login should work', async ({ page }) => {
        await login(page, { prefix: `/${tenant.prefix}` });
        await page.goto(`/${tenant.prefix}/Admin`);
        await expect(page.locator('.menu-admin')).toHaveAttribute('id', 'adminMenu');
    });
});
