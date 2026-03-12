import { Page } from "@playwright/test";

export interface LoginOptions {
  username?: string;
  password?: string;
  prefix?: string;
}

const defaultCredentials = {
  username: "admin",
  password: "Orchard1!",
};

/**
 * Log in to the OrchardCore admin panel.
 */
export async function login(
  page: Page,
  options: LoginOptions = {},
): Promise<void> {
  const username = options.username ?? defaultCredentials.username;
  const password = options.password ?? defaultCredentials.password;
  const prefix = options.prefix ?? "";

  await page.goto(`${prefix}/login`);

  // If already authenticated, the login page redirects away — no form is shown
  const loginForm = page.locator("#LoginForm_UserName");
  if (await loginForm.isVisible({ timeout: 3000 }).catch(() => false)) {
    await loginForm.fill(username);
    await page.fill("#LoginForm_Password", password);
    await loginForm.locator("xpath=ancestor::form").locator('button[type="submit"]').click();
    await page.waitForLoadState('load');
  } else {
    // Already logged in, navigate to Admin directly
    await page.goto(`${prefix}/Admin`);
  }
}
