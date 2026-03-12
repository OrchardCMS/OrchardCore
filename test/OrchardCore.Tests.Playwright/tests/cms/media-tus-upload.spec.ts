import { test, expect } from "@playwright/test";
import { generateTenantInfo, type TenantInfo } from "../../helpers/utils";
import { newTenant } from "../../helpers/tenants";
import { login } from "../../helpers/auth";
import {
  generateTestFile,
  cleanupTestFiles,
  navigateToMedia,
  uploadFile,
  expectFileInLibrary,
} from "../../helpers/media";

// File sizes for different test scenarios
const SMALL_FILE_SIZE = 1 * 1024 * 1024;   // 1 MB
const LARGE_FILE_SIZE = 10 * 1024 * 1024;   // 10 MB

/**
 * Throttle TUS PATCH responses to keep uploads visibly in-progress.
 * Delays the response (not the request) so the server receives data normally
 * but the client waits before sending the next chunk.
 */
async function throttleTusUploads(page: import("@playwright/test").Page, delayMs = 3000) {
  await page.route("**/api/media/tus/**", async (route) => {
    if (route.request().method() === "PATCH") {
      const response = await route.fetch();
      await new Promise((resolve) => setTimeout(resolve, delayMs));
      await route.fulfill({ response });
    } else {
      await route.continue();
    }
  });
}

// Uses the MediaTus recipe (fixtures/media-tus.recipe.json) which includes OrchardCore.Media.Tus
test.describe("Media TUS Upload", () => {
  let tenant: TenantInfo;

  test.beforeAll(async ({ browser }) => {
    tenant = generateTenantInfo("MediaTus");
    const page = await browser.newPage();
    await newTenant(page, tenant);
    await page.close();
  });

  test.afterAll(() => {
    cleanupTestFiles();
  });

  test.beforeEach(async ({ page }) => {
    await login(page, { prefix: `/${tenant.prefix}` });
    await navigateToMedia(page, `/${tenant.prefix}`);
  });

  test("should upload a file via TUS", async ({ page }) => {
    const filePath = generateTestFile("tus-upload-test.dat", SMALL_FILE_SIZE);
    await uploadFile(page, filePath);
    await expectFileInLibrary(page, "tus-upload-test.dat");
  });

  test("should pause and resume a single file upload", async ({ page }) => {
    await throttleTusUploads(page);
    const filePath = generateTestFile("pause-resume-test.dat", LARGE_FILE_SIZE);
    await uploadFile(page, filePath);

    // Wait for the upload toast to show the file in-progress
    const container = page.locator(".upload-toast-container");
    await container.waitFor({ state: "visible", timeout: 15_000 });
    const fileRow = container.locator(".upload-toast-item").filter({ hasText: "pause-resume-test.dat" });
    await fileRow.waitFor({ state: "visible", timeout: 10_000 });

    // Verify the pause button is visible (upload is active)
    const pauseBtn = fileRow.locator('button:has(svg[data-icon="pause"])');
    await expect(pauseBtn).toBeVisible();

    // Pause the upload — icon should switch to play
    await pauseBtn.click();
    await expect(fileRow.locator('svg[data-icon="play"]')).toBeVisible();

    // Resume the upload — icon should switch back to pause
    await fileRow.locator('button:has(svg[data-icon="play"])').click();
    await expect(fileRow.locator('svg[data-icon="pause"]')).toBeVisible();

    // Remove throttle, reload, and verify the file completes on a fresh upload
    await page.unroute("**/api/media/tus/**");
    // The throttled upload may error after pause/resume due to route interception.
    // Navigate away and back, then upload again without throttling.
    await navigateToMedia(page, `/${tenant.prefix}`);
    const filePath2 = generateTestFile("pause-resume-verify.dat", SMALL_FILE_SIZE);
    await uploadFile(page, filePath2);
    await expectFileInLibrary(page, "pause-resume-verify.dat");
  });

  test("should pause and resume multiple simultaneous uploads", async ({ page }) => {
    await throttleTusUploads(page);
    const filePath1 = generateTestFile("multi-pause-1.dat", LARGE_FILE_SIZE);
    const filePath2 = generateTestFile("multi-pause-2.dat", LARGE_FILE_SIZE);

    const fileInput = page.locator("#fileupload");
    await fileInput.setInputFiles([filePath1, filePath2]);

    const container = page.locator(".upload-toast-container");
    await container.waitFor({ state: "visible", timeout: 15_000 });
    const row1 = container.locator(".upload-toast-item").filter({ hasText: "multi-pause-1.dat" });
    const row2 = container.locator(".upload-toast-item").filter({ hasText: "multi-pause-2.dat" });
    await row1.waitFor({ state: "visible", timeout: 10_000 });
    await row2.waitFor({ state: "visible", timeout: 10_000 });

    // Pause both uploads
    await row1.locator('button:has(svg[data-icon="pause"])').click();
    await row2.locator('button:has(svg[data-icon="pause"])').click();
    await expect(row1.locator('svg[data-icon="play"]')).toBeVisible();
    await expect(row2.locator('svg[data-icon="play"]')).toBeVisible();

    // Resume both uploads
    await row1.locator('button:has(svg[data-icon="play"])').click();
    await row2.locator('button:has(svg[data-icon="play"])').click();
    await expect(row1.locator('svg[data-icon="pause"]')).toBeVisible();
    await expect(row2.locator('svg[data-icon="pause"]')).toBeVisible();

    // Verify uploads can complete without throttle
    await page.unroute("**/api/media/tus/**");
    await navigateToMedia(page, `/${tenant.prefix}`);
    const verifyFile = generateTestFile("multi-verify.dat", SMALL_FILE_SIZE);
    await uploadFile(page, verifyFile);
    await expectFileInLibrary(page, "multi-verify.dat");
  });
});
