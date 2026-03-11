import { type Page } from '@playwright/test';

export async function btnCreateClick(page: Page): Promise<void> {
    await page.locator('.btn.create').click();
}

export async function btnSaveClick(page: Page): Promise<void> {
    await page.locator('.btn.save').click();
}

export async function btnSaveContinueClick(page: Page): Promise<void> {
    await page.locator('.dropdown-item.save-continue').click();
}

export async function btnCancelClick(page: Page): Promise<void> {
    await page.locator('.btn.cancel').click();
}

export async function btnPublishClick(page: Page): Promise<void> {
    await page.locator('.btn.public').click();
}

export async function btnPublishContinueClick(page: Page): Promise<void> {
    await page.locator('.dropdown-item.publish-continue').click();
}

export async function btnModalOkClick(page: Page): Promise<void> {
    await page.locator('#modalOkButton').click();
}
