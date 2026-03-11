import { type Page } from '@playwright/test';
import { defaultOrchardConfig, type OrchardConfig } from './utils';

export async function login(page: Page, options: { prefix?: string; config?: OrchardConfig } = {}): Promise<void> {
    const { prefix = '', config = defaultOrchardConfig } = options;
    await page.goto(`${prefix}/login`);

    // If already logged in (redirected away from login), skip
    if (!page.url().toLowerCase().includes('/login')) {
        return;
    }

    await page.locator('#LoginForm_UserName').fill(config.username);
    await page.locator('#LoginForm_Password').fill(config.password);
    await page.locator('button[type="submit"]').click();
    await page.waitForLoadState('networkidle');
}
