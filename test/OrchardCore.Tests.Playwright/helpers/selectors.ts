import { type Page, type Locator } from '@playwright/test';

export function getByCy(page: Page, selector: string, exact: boolean = false): Locator {
    if (exact) {
        return page.locator(`[data-cy="${selector}"]`);
    }
    return page.locator(`[data-cy^="${selector}"]`);
}

export function findByCy(locator: Locator, selector: string, exact: boolean = false): Locator {
    if (exact) {
        return locator.locator(`[data-cy="${selector}"]`);
    }
    return locator.locator(`[data-cy^="${selector}"]`);
}
