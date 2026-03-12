import { Page, expect } from "@playwright/test";
import * as fs from "fs";
import * as path from "path";
import * as os from "os";

const TEMP_DIR = path.join(os.tmpdir(), "playwright-test-files");

/**
 * Generates a test file of a given size filled with random data.
 * Returns the absolute path to the generated file.
 */
export function generateTestFile(name: string, sizeInBytes: number): string {
  if (!fs.existsSync(TEMP_DIR)) {
    fs.mkdirSync(TEMP_DIR, { recursive: true });
  }

  const filePath = path.join(TEMP_DIR, name);

  // Write in 1MB chunks to avoid excessive memory use
  const chunkSize = 1024 * 1024;
  const fd = fs.openSync(filePath, "w");
  let remaining = sizeInBytes;

  while (remaining > 0) {
    const size = Math.min(chunkSize, remaining);
    const buffer = Buffer.alloc(size, 0x41); // Fill with 'A'
    fs.writeSync(fd, buffer);
    remaining -= size;
  }

  fs.closeSync(fd);
  return filePath;
}

/**
 * Cleans up all generated test files.
 */
export function cleanupTestFiles(): void {
  if (fs.existsSync(TEMP_DIR)) {
    fs.rmSync(TEMP_DIR, { recursive: true, force: true });
  }
}

/**
 * Navigate to the Media admin page.
 */
export async function navigateToMedia(page: Page, prefix = ""): Promise<void> {
  await page.goto(`${prefix}/Admin/Media`);
  // Wait for the Vue media app to mount and render the toolbar
  await page.locator('text=Media Library').first().waitFor({ state: 'visible', timeout: 30_000 });
}

/**
 * Navigate to a specific folder in the media library.
 */
export async function navigateToFolder(page: Page, folderName: string): Promise<void> {
  await page.locator(`.treenode-content:has-text("${folderName}")`).click();
  await page.waitForTimeout(500);
}

/**
 * Upload a file using the file input (triggers Uppy).
 */
export async function uploadFile(page: Page, filePath: string): Promise<void> {
  const fileInput = page.locator("#fileupload");
  await fileInput.setInputFiles(filePath);
}

/**
 * Check that a file appears in the media library file list.
 */
export async function expectFileInLibrary(page: Page, fileName: string, timeoutMs = 30_000): Promise<void> {
  // The media library may need a page refresh to show newly uploaded files.
  // Try finding the file first, then reload and check again if not found.
  const locator = page.getByText(fileName, { exact: true }).first();
  try {
    await expect(locator).toBeVisible({ timeout: timeoutMs / 2 });
  } catch {
    await page.reload();
    await page.locator('text=Media Library').first().waitFor({ state: 'visible', timeout: 15_000 });
    await expect(locator).toBeVisible({ timeout: timeoutMs / 2 });
  }
}

/**
 * Delete a file from the media library via the UI.
 */
export async function deleteFileFromLibrary(page: Page, fileName: string): Promise<void> {
  // Click the kebab menu for the file
  const fileRow = page.locator("tr, .media-item").filter({ hasText: fileName });
  await fileRow.locator(".btn-link, [data-bs-toggle='dropdown']").last().click();
  // Click delete option
  await page.locator("text=Delete").click();
  // Confirm deletion if modal appears
  const confirmBtn = page.locator(".modal .btn-danger, .modal .btn-primary").first();
  if (await confirmBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
    await confirmBtn.click();
  }
}

/**
 * Create a folder in the media library.
 */
export async function createFolder(page: Page, folderName: string): Promise<void> {
  // Right-click or use the create folder button
  await page.locator(".btn-create-folder, [title='Create folder']").first().click();
  await page.fill("input[name='folderName'], .modal input[type='text']", folderName);
  await page.locator(".modal .btn-primary, button:has-text('Ok')").click();
  await page.waitForTimeout(500);
}
