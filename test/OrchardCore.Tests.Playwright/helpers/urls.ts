import { type Page } from '@playwright/test';

export async function visitContentPage(page: Page, prefix: string, contentItemId: string): Promise<void> {
    await page.goto(`${prefix}/Contents/ContentItems/${contentItemId}`);
}
