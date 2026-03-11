import { type Page } from '@playwright/test';
import { expect } from '@playwright/test';
import { btnModalOkClick } from './buttons';
import fs from 'fs';

export async function runRecipe(page: Page, prefix: string, recipeName: string): Promise<void> {
    await page.goto(`${prefix}/Admin/Recipes`);
    await page.locator(`#btn-run-${recipeName}`).click();
    await btnModalOkClick(page);
}

export async function uploadRecipeJson(page: Page, prefix: string, fixturePath: string): Promise<void> {
    const data = JSON.parse(fs.readFileSync(fixturePath, 'utf-8'));
    await page.goto(`${prefix}/Admin/DeploymentPlan/Import/Json`);
    await page.locator('.CodeMirror').waitFor({ state: 'visible' });
    await page.evaluate((json: string) => {
        const cm = document.querySelector('.CodeMirror') as any;
        cm.CodeMirror.setValue(json);
    }, JSON.stringify(data));
    await page.locator('.ta-content > form').evaluate((form: HTMLFormElement) => form.submit());
    await expect(page.locator('.message-success')).toContainText('Recipe imported');
}
