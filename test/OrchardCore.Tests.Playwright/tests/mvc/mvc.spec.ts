import { test, expect } from '@playwright/test';

test.describe('MVC Application', () => {
    test('should display Hello World', async ({ page }) => {
        await page.goto('/');
        await expect(page.locator('body')).toContainText('Hello World');
    });
});
