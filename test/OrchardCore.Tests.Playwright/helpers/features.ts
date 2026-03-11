import { type Page } from '@playwright/test';

export async function enableFeature(page: Page, prefix: string, featureName: string): Promise<void> {
    await page.goto(`${prefix}/Admin/Features`);
    await page.locator(`#btn-enable-${featureName}`).click();
}

export async function disableFeature(page: Page, prefix: string, featureName: string): Promise<void> {
    await page.goto(`${prefix}/Admin/Features`);
    await page.locator(`#btn-disable-${featureName}`).click();
}
