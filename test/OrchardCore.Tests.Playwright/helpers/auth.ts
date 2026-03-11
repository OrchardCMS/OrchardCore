import { Page } from "@playwright/test";

export interface AdminCredentials {
  username: string;
  password: string;
}

const defaultCredentials: AdminCredentials = {
  username: "admin",
  password: "Orchard1!",
};

/**
 * Log in to the OrchardCore admin panel.
 */
export async function login(
  page: Page,
  credentials: AdminCredentials = defaultCredentials,
): Promise<void> {
  await page.goto("/login");
  await page.fill("#LoginForm_UserName", credentials.username);
  await page.fill("#LoginForm_Password", credentials.password);
  await page.locator("#LoginForm_UserName").locator("xpath=ancestor::form").locator('button[type="submit"]').click();
  await page.waitForURL("**/Admin");
}
