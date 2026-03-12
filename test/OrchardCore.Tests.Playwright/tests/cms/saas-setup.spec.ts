import { test, expect } from '@playwright/test';
import { login } from '../../helpers/auth';
import { siteSetup } from '../../helpers/tenants';
import { setPageSize } from '../../helpers/configuration';

const sassSite = {
    name: 'Testing SaaS',
    prefix: '',
    setupRecipe: 'SaaS',
};

test.describe('Setup SaaS', () => {
    test('Successfully setup the SaaS default tenant', async ({ page }) => {
        await page.goto('/');
        await siteSetup(page, sassSite);
        await login(page, { prefix: sassSite.prefix });
        await setPageSize(page, sassSite.prefix, '100');
    });
});
