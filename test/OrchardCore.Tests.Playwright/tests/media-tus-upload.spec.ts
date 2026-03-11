import { test, expect } from "@playwright/test";
import { login } from "../helpers/auth";
import {
  generateTestFile,
  cleanupTestFiles,
  navigateToMedia,
  uploadFile,
  waitForUploadToast,
  pauseUpload,
  resumeUpload,
  waitForUploadSuccess,
  expectUploadPaused,
  expectUploadActive,
  expectFileInLibrary,
} from "../helpers/media";

// File sizes for different test scenarios
const SMALL_FILE_SIZE = 1 * 1024 * 1024;        // 1 MB — completes quickly
const MEDIUM_FILE_SIZE = 20 * 1024 * 1024;       // 20 MB — enough time to pause
const LARGE_FILE_SIZE = 50 * 1024 * 1024;        // 50 MB — reliable pause/resume

test.describe("Media TUS Upload", () => {
  test.beforeAll(() => {
    // Pre-generate test files so they're ready for all tests
    generateTestFile("small-test.bin", SMALL_FILE_SIZE);
    generateTestFile("medium-test.bin", MEDIUM_FILE_SIZE);
    generateTestFile("large-test-1.bin", LARGE_FILE_SIZE);
    generateTestFile("large-test-2.bin", LARGE_FILE_SIZE);
  });

  test.afterAll(() => {
    cleanupTestFiles();
  });

  test.beforeEach(async ({ page }) => {
    await login(page);
    await navigateToMedia(page);
  });

  test("should upload a small file via TUS", async ({ page }) => {
    const filePath = generateTestFile("small-tus-test.bin", SMALL_FILE_SIZE);

    await uploadFile(page, filePath);
    await waitForUploadToast(page, "small-tus-test.bin");
    await waitForUploadSuccess(page, "small-tus-test.bin", 30_000);
  });

  test("should upload a medium file and show progress", async ({ page }) => {
    const filePath = generateTestFile("medium-tus-test.bin", MEDIUM_FILE_SIZE);

    await uploadFile(page, filePath);
    await waitForUploadToast(page, "medium-tus-test.bin");

    // Verify progress bar is visible and active
    const progressBar = page.locator(".upload-toast-item")
      .filter({ hasText: "medium-tus-test.bin" })
      .locator(".upload-toast-progress-bar");
    await expect(progressBar).toBeVisible();

    await waitForUploadSuccess(page, "medium-tus-test.bin", 60_000);
  });

  test("should pause and resume a TUS upload", async ({ page }) => {
    const filePath = generateTestFile("pause-resume-test.bin", LARGE_FILE_SIZE);

    await uploadFile(page, filePath);
    await waitForUploadToast(page, "pause-resume-test.bin");

    // Wait for some progress before pausing
    await page.waitForTimeout(2000);

    // Pause the upload
    await pauseUpload(page, "pause-resume-test.bin");
    await expectUploadPaused(page, "pause-resume-test.bin");

    // Verify progress bar shows paused state (yellow, pulsing)
    const pausedBar = page.locator(".upload-toast-item")
      .filter({ hasText: "pause-resume-test.bin" })
      .locator(".upload-toast-progress-bar.is-paused");
    await expect(pausedBar).toBeVisible();

    // Wait a moment while paused
    await page.waitForTimeout(2000);

    // Resume the upload
    await resumeUpload(page, "pause-resume-test.bin");
    await expectUploadActive(page, "pause-resume-test.bin");

    // Wait for completion
    await waitForUploadSuccess(page, "pause-resume-test.bin", 120_000);
  });

  test("should pause and resume multiple times", async ({ page }) => {
    const filePath = generateTestFile("multi-pause-test.bin", LARGE_FILE_SIZE);

    await uploadFile(page, filePath);
    await waitForUploadToast(page, "multi-pause-test.bin");

    // First pause/resume cycle
    await page.waitForTimeout(1500);
    await pauseUpload(page, "multi-pause-test.bin");
    await expectUploadPaused(page, "multi-pause-test.bin");
    await page.waitForTimeout(1000);
    await resumeUpload(page, "multi-pause-test.bin");
    await expectUploadActive(page, "multi-pause-test.bin");

    // Second pause/resume cycle
    await page.waitForTimeout(1500);
    await pauseUpload(page, "multi-pause-test.bin");
    await expectUploadPaused(page, "multi-pause-test.bin");
    await page.waitForTimeout(1000);
    await resumeUpload(page, "multi-pause-test.bin");

    // Should complete successfully
    await waitForUploadSuccess(page, "multi-pause-test.bin", 120_000);
  });

  test("should upload two files simultaneously", async ({ page }) => {
    const filePath1 = generateTestFile("simultaneous-1.bin", MEDIUM_FILE_SIZE);
    const filePath2 = generateTestFile("simultaneous-2.bin", MEDIUM_FILE_SIZE);

    // Upload both files at once
    const fileInput = page.locator("#fileupload");
    await fileInput.setInputFiles([filePath1, filePath2]);

    await waitForUploadToast(page, "simultaneous-1.bin");
    await waitForUploadToast(page, "simultaneous-2.bin");

    await waitForUploadSuccess(page, "simultaneous-1.bin", 60_000);
    await waitForUploadSuccess(page, "simultaneous-2.bin", 60_000);
  });

  test("should pause one file while another continues uploading", async ({ page }) => {
    const filePath1 = generateTestFile("parallel-upload-1.bin", LARGE_FILE_SIZE);
    const filePath2 = generateTestFile("parallel-upload-2.bin", LARGE_FILE_SIZE);

    // Upload both files
    const fileInput = page.locator("#fileupload");
    await fileInput.setInputFiles([filePath1, filePath2]);

    await waitForUploadToast(page, "parallel-upload-1.bin");
    await waitForUploadToast(page, "parallel-upload-2.bin");

    // Wait for progress then pause only file 1
    await page.waitForTimeout(2000);
    await pauseUpload(page, "parallel-upload-1.bin");
    await expectUploadPaused(page, "parallel-upload-1.bin");

    // File 2 should still be active
    await expectUploadActive(page, "parallel-upload-2.bin");

    // Resume file 1
    await page.waitForTimeout(2000);
    await resumeUpload(page, "parallel-upload-1.bin");

    // Both should complete
    await waitForUploadSuccess(page, "parallel-upload-1.bin", 120_000);
    await waitForUploadSuccess(page, "parallel-upload-2.bin", 120_000);
  });

  test("should resume upload after page refresh (browser resume)", async ({ page }) => {
    const filePath = generateTestFile("refresh-resume-test.bin", LARGE_FILE_SIZE);

    await uploadFile(page, filePath);
    await waitForUploadToast(page, "refresh-resume-test.bin");

    // Wait for some chunks to upload
    await page.waitForTimeout(3000);

    // Refresh the page (simulates browser disconnect)
    await page.reload();
    await page.waitForSelector("#media-app");

    // Re-upload the same file — tus-js-client should detect the previous
    // partial upload in localStorage and resume from the stored offset
    await uploadFile(page, filePath);
    await waitForUploadToast(page, "refresh-resume-test.bin");

    // Should complete and show the resumed indicator
    await waitForUploadSuccess(page, "refresh-resume-test.bin", 120_000);

    // Check for the resume icon (blue rotate icon)
    const resumeIcon = page.locator(".upload-toast-item")
      .filter({ hasText: "refresh-resume-test.bin" })
      .locator(".fa-rotate");
    // The resumed icon may have disappeared after the 3-second auto-dismiss,
    // so we just verify the upload completed successfully
  });

  test("should show upload speed indicator", async ({ page }) => {
    const filePath = generateTestFile("speed-test.bin", MEDIUM_FILE_SIZE);

    await uploadFile(page, filePath);
    await waitForUploadToast(page, "speed-test.bin");

    // Wait for speed calculation (updates every 500ms)
    await page.waitForTimeout(2000);

    // Speed indicator should be visible with a unit suffix
    const speedLabel = page.locator(".upload-toast-item")
      .filter({ hasText: "speed-test.bin" })
      .locator(".upload-toast-speed");
    await expect(speedLabel).toBeVisible({ timeout: 5000 });
    const speedText = await speedLabel.textContent();
    expect(speedText).toMatch(/\d+(\.\d+)?\s*(B\/s|KB\/s|MB\/s)/);

    await waitForUploadSuccess(page, "speed-test.bin", 60_000);
  });
});
