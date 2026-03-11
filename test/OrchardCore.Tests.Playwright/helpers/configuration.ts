import { type Page } from '@playwright/test';
import { btnSaveClick } from './buttons';

export async function setPageSize(page: Page, prefix: string, size: string): Promise<void> {
    await page.goto(`${prefix}/Admin/Settings/general`);
    await page.locator('#ISite_PageSize').clear();
    await page.locator('#ISite_PageSize').fill(size);
    await btnSaveClick(page);
    await page.locator('.message-success').waitFor();
}
