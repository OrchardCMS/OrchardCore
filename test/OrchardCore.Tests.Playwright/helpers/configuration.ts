import { type Page } from '@playwright/test';

export async function setPageSize(page: Page, prefix: string, size: string): Promise<void> {
    await page.goto(`${prefix}/Admin/Settings/general`);
    await page.locator('#ISite_PageSize').fill(size);
    await page.locator('button.save[type="submit"]').click();
    await page.waitForLoadState('networkidle');
}
