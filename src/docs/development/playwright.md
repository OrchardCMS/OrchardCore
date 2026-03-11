# Cypress to Playwright Migration Plan

This document outlines the plan to migrate OrchardCore's functional tests from Cypress (in `test/OrchardCore.Tests.Functional/`) to Playwright (in `test/OrchardCore.Tests.Playwright/`), and to update the GitHub CI workflows accordingly.

---

## Table of Contents

1. [Current State](#current-state)
2. [Target State](#target-state)
3. [Phase 1 — Helpers & Utilities](#phase-1--helpers--utilities)
4. [Phase 2 — Test Migration](#phase-2--test-migration)
5. [Phase 3 — Test Runner & App Lifecycle](#phase-3--test-runner--app-lifecycle)
6. [Phase 4 — CI Workflow Updates](#phase-4--ci-workflow-updates)
7. [Phase 5 — Cleanup](#phase-5--cleanup)
8. [Command Mapping Reference](#command-mapping-reference)
9. [Multi-Database Support](#multi-database-support)

---

## Current State

### Cypress Tests (`test/OrchardCore.Tests.Functional/`)

**Structure:**
```
test/OrchardCore.Tests.Functional/
├── cms-tests/
│   ├── cypress/integration/
│   │   ├── 000-saas-setup.js         # Default SaaS tenant setup
│   │   ├── agency-test.js            # Agency theme
│   │   ├── blog-test.js              # Blog recipe
│   │   ├── comingsoon-test.js        # ComingSoon theme
│   │   ├── headless-test.js          # Headless recipe
│   │   ├── migrations-test.js        # Feature migrations
│   │   └── sass-test.js              # SaaS recipe (secondary tenant)
│   ├── cypress.json
│   └── Recipes/migrations.recipe.json
├── mvc-tests/
│   ├── cypress/integration/
│   │   └── mvc-test.js               # Basic MVC smoke test
│   └── cypress.json
├── cypress-commands/                  # Shared "cypress-orchardcore" package
│   └── src/
│       ├── buttons.js
│       ├── configuration.js
│       ├── cySelectors.js
│       ├── features.js
│       ├── recipe.js
│       ├── tenants.js
│       ├── urls.js
│       ├── utils.js
│       └── test-runner.js
└── package.json
```

**Test count:** 8 test files, ~15 test cases total.

**Key patterns:**
- Tenant-per-test isolation via `generateTenantInfo()` + `cy.newTenant()`
- Custom Cypress commands for admin operations (login, tenant CRUD, feature management, recipe deployment)
- `test-runner.js` manages app lifecycle: build → delete App_Data → host → run Cypress → kill server
- Base URL: `http://localhost:5000`
- Credentials: admin / Orchard1!

### Existing Playwright Setup (`test/OrchardCore.Tests.Playwright/`)

```
test/OrchardCore.Tests.Playwright/
├── helpers/
│   ├── auth.ts          # login()
│   └── media.ts         # Media library utilities
├── tests/
│   └── media-tus-upload.spec.ts
├── playwright.config.ts
├── tsconfig.json
└── package.json
```

- Playwright v1.50+ (installed 1.58.2)
- Base URL: `http://localhost:5001` (via `ORCHARD_URL` env var)
- Single worker, serial execution
- HTML reporter, screenshots on failure

---

## Target State

After migration, all functional tests live in `test/OrchardCore.Tests.Playwright/`:

```
test/OrchardCore.Tests.Playwright/
├── helpers/
│   ├── auth.ts                  # Login utilities (exists)
│   ├── media.ts                 # Media utilities (exists)
│   ├── tenants.ts               # NEW — Tenant creation & management
│   ├── features.ts              # NEW — Enable/disable features
│   ├── recipes.ts               # NEW — Recipe execution & upload
│   ├── configuration.ts         # NEW — Site configuration helpers
│   ├── selectors.ts             # NEW — data-cy and common selectors
│   ├── buttons.ts               # NEW — Common admin button actions
│   ├── urls.ts                  # NEW — URL navigation helpers
│   └── app-lifecycle.ts         # NEW — Build, host, teardown OrchardCore
├── tests/
│   ├── media-tus-upload.spec.ts # Existing
│   ├── cms/
│   │   ├── saas-setup.spec.ts   # Migrated from 000-saas-setup.js
│   │   ├── agency.spec.ts       # Migrated from agency-test.js
│   │   ├── blog.spec.ts         # Migrated from blog-test.js
│   │   ├── comingsoon.spec.ts   # Migrated from comingsoon-test.js
│   │   ├── headless.spec.ts     # Migrated from headless-test.js
│   │   ├── migrations.spec.ts   # Migrated from migrations-test.js
│   │   └── saas.spec.ts         # Migrated from sass-test.js
│   └── mvc/
│       └── mvc.spec.ts          # Migrated from mvc-test.js
├── fixtures/
│   └── migrations.recipe.json   # Copied from cms-tests/Recipes/
├── playwright.config.ts
├── global-setup.ts              # NEW — Orchestrate app build/host
├── global-teardown.ts           # NEW — Kill server, cleanup
├── tsconfig.json
└── package.json
```

---

## Phase 1 — Helpers & Utilities

Migrate all custom Cypress commands into Playwright helper modules. Each helper is a plain TypeScript module exporting async functions that accept a Playwright `Page` object.

### 1.1 Tenant Management (`helpers/tenants.ts`)

Migrate from `cypress-commands/src/tenants.js` and `cypress-commands/src/utils.js`.

| Cypress Command | Playwright Function |
|---|---|
| `generateTenantInfo(recipe, desc)` | `generateTenantInfo(recipe: string, desc?: string): TenantInfo` |
| `cy.newTenant(tenantInfo)` | `newTenant(page: Page, tenantInfo: TenantInfo): Promise<void>` |
| `cy.createTenant(tenantInfo)` | `createTenant(page: Page, tenantInfo: TenantInfo): Promise<void>` |
| `cy.visitTenantSetupPage(tenantInfo)` | `visitTenantSetupPage(page: Page, tenantInfo: TenantInfo): Promise<void>` |
| `cy.siteSetup(tenantInfo)` | `siteSetup(page: Page, tenantInfo: TenantInfo): Promise<void>` |
| `cy.login(tenantInfo)` | Update existing `login()` in `auth.ts` to accept optional `TenantInfo` |

**Key considerations:**
- `generateTenantInfo()` uses a timestamp-based prefix to ensure uniqueness — preserve this pattern.
- Tenant creation involves navigating to the admin, filling a form, and submitting. Use `page.locator()` and `page.fill()`.
- `cy.login()` reads credentials from `cypress.json` → use environment variables or a config constant.

### 1.2 Recipe Helpers (`helpers/recipes.ts`)

Migrate from `cypress-commands/src/recipe.js`.

| Cypress Command | Playwright Function |
|---|---|
| `cy.runRecipe(tenantInfo, recipeName)` | `runRecipe(page: Page, tenantInfo: TenantInfo, recipeName: string): Promise<void>` |
| `cy.uploadRecipeJson(tenantInfo, fixturePath)` | `uploadRecipeJson(page: Page, tenantInfo: TenantInfo, fixturePath: string): Promise<void>` |

### 1.3 Feature Management (`helpers/features.ts`)

Migrate from `cypress-commands/src/features.js`.

| Cypress Command | Playwright Function |
|---|---|
| `cy.enableFeature(tenantInfo, featureName)` | `enableFeature(page: Page, tenantInfo: TenantInfo, featureName: string): Promise<void>` |
| `cy.disableFeature(tenantInfo, featureName)` | `disableFeature(page: Page, tenantInfo: TenantInfo, featureName: string): Promise<void>` |

### 1.4 Configuration (`helpers/configuration.ts`)

Migrate from `cypress-commands/src/configuration.js`.

| Cypress Command | Playwright Function |
|---|---|
| `cy.setPageSize(tenantInfo, size)` | `setPageSize(page: Page, tenantInfo: TenantInfo, size: number): Promise<void>` |

### 1.5 Button Helpers (`helpers/buttons.ts`)

Migrate from `cypress-commands/src/buttons.js`.

| Cypress Command | Playwright Function |
|---|---|
| `cy.btnCreateClick()` | `btnCreateClick(page: Page): Promise<void>` |
| `cy.btnSaveClick()` | `btnSaveClick(page: Page): Promise<void>` |
| `cy.btnSaveContinueClick()` | `btnSaveContinueClick(page: Page): Promise<void>` |
| `cy.btnCancelClick()` | `btnCancelClick(page: Page): Promise<void>` |
| `cy.btnPublishClick()` | `btnPublishClick(page: Page): Promise<void>` |
| `cy.btnPublishContinueClick()` | `btnPublishContinueClick(page: Page): Promise<void>` |
| `cy.btnModalOkClick()` | `btnModalOkClick(page: Page): Promise<void>` |

### 1.6 Selectors (`helpers/selectors.ts`)

Migrate from `cypress-commands/src/cySelectors.js`.

| Cypress Command | Playwright Function |
|---|---|
| `cy.getByCy(selector)` | `getByCy(page: Page, selector: string): Locator` |
| `cy.findByCy(selector)` | Use `locator.locator('[data-cy=...]')` chaining directly |

### 1.7 URL Helpers (`helpers/urls.ts`)

Migrate from `cypress-commands/src/urls.js`.

| Cypress Command | Playwright Function |
|---|---|
| `cy.visitContentPage(tenantInfo, id)` | `visitContentPage(page: Page, tenantInfo: TenantInfo, contentItemId: string): Promise<void>` |

---

## Phase 2 — Test Migration

Migrate each Cypress test file to a Playwright `.spec.ts` file. Tests should be migrated in order, since some (like `000-saas-setup.js`) set up state used by others.

### 2.1 CMS Tests

#### `tests/cms/saas-setup.spec.ts` (from `000-saas-setup.js`)

**Purpose:** Initializes the default SaaS tenant (must run first).

```typescript
// Pattern:
import { test, expect } from '@playwright/test';
import { login } from '../../helpers/auth';
import { generateTenantInfo, newTenant, siteSetup } from '../../helpers/tenants';

test.describe('SaaS Setup', () => {
    let tenant: TenantInfo;

    test.beforeAll(async ({ browser }) => {
        const page = await browser.newPage();
        tenant = generateTenantInfo('SaaS');
        await newTenant(page, tenant);
        await page.close();
    });

    test('should setup the default tenant', async ({ page }) => {
        await login(page, tenant);
        // assertions...
    });
});
```

#### Remaining CMS Tests

Each follows the same pattern — create a tenant with a specific recipe, then assert:

| Source File | Target File | Recipe |
|---|---|---|
| `agency-test.js` | `tests/cms/agency.spec.ts` | Agency |
| `blog-test.js` | `tests/cms/blog.spec.ts` | Blog |
| `comingsoon-test.js` | `tests/cms/comingsoon.spec.ts` | ComingSoon |
| `headless-test.js` | `tests/cms/headless.spec.ts` | Headless |
| `migrations-test.js` | `tests/cms/migrations.spec.ts` | Custom migrations recipe |
| `sass-test.js` | `tests/cms/saas.spec.ts` | SaaS |

**Migration checklist per test:**
- [ ] Replace `describe()` → `test.describe()`
- [ ] Replace `before()` → `test.beforeAll()`
- [ ] Replace `it()` → `test()`
- [ ] Replace `cy.visit()` → `page.goto()`
- [ ] Replace `cy.get()` → `page.locator()`
- [ ] Replace `cy.contains()` → `page.locator(':has-text("...")')` or `page.getByText()`
- [ ] Replace `cy.should('contain', ...)` → `await expect(locator).toContainText(...)`
- [ ] Replace `cy.should('be.visible')` → `await expect(locator).toBeVisible()`
- [ ] Replace `cy.url().should('contain', ...)` → `await expect(page).toHaveURL(...)`
- [ ] Replace custom `cy.*` commands → imported helper function calls

### 2.2 MVC Test

#### `tests/mvc/mvc.spec.ts` (from `mvc-test.js`)

**Purpose:** Basic smoke test that the MVC app starts and serves "Hello World".

```typescript
import { test, expect } from '@playwright/test';

test.describe('MVC Application', () => {
    test('should display Hello World', async ({ page }) => {
        await page.goto('/');
        await expect(page.locator('body')).toContainText('Hello World');
    });
});
```

**Note:** The MVC test requires a different application (`OrchardCore.Mvc.Web`). This must be handled in the test runner / CI configuration (see Phase 3).

---

## Phase 3 — Test Runner & App Lifecycle

The current Cypress setup uses `test-runner.js` to build, host, and teardown the .NET application. We need an equivalent for Playwright.

### 3.1 App Lifecycle Helper (`helpers/app-lifecycle.ts`)

Create a utility that:
1. Builds the .NET application in Release mode.
2. Deletes the `App_Data_Tests` directory for a clean state.
3. Spawns the application as a child process.
4. Waits for it to be ready (HTTP health check on the base URL).
5. Returns a handle to kill the process on teardown.

```typescript
import { execSync, spawn, ChildProcess } from 'child_process';
import fs from 'fs-extra';
import path from 'path';

export function buildApp(appDir: string): void { /* dotnet build -c Release */ }
export function deleteAppData(appDir: string): void { /* rm App_Data_Tests */ }
export function hostApp(appDir: string, assembly: string): ChildProcess { /* spawn dotnet */ }
export async function waitForReady(baseUrl: string, timeoutMs?: number): Promise<void> { /* poll GET / */ }
export function killApp(proc: ChildProcess): void { /* proc.kill() */ }
```

### 3.2 Global Setup & Teardown

Use Playwright's `globalSetup` and `globalTeardown` in `playwright.config.ts`:

```typescript
// playwright.config.ts
export default defineConfig({
    globalSetup: './global-setup.ts',
    globalTeardown: './global-teardown.ts',
    // ...
});
```

**`global-setup.ts`:**
- Builds OrchardCore.Cms.Web (or whichever app)
- Deletes App_Data_Tests
- Starts the server
- Waits for ready
- Stores the process handle in a global variable or temp file

**`global-teardown.ts`:**
- Kills the server process
- Cleans up temp files

### 3.3 Update `package.json` Scripts

```json
{
  "scripts": {
    "test": "npx playwright test",
    "test:cms": "npx playwright test tests/cms/",
    "test:mvc": "ORCHARD_APP=mvc npx playwright test tests/mvc/",
    "test:media": "npx playwright test tests/media-tus-upload.spec.ts",
    "test:ui": "npx playwright test --ui",
    "test:headed": "npx playwright test --headed",
    "test:debug": "npx playwright test --debug",
    "install:browsers": "npx playwright install chromium"
  }
}
```

### 3.4 Playwright Config Updates

Update `playwright.config.ts` to:
- Add `globalSetup` and `globalTeardown`
- Add test projects for CMS and MVC if needed
- Configure serial execution for CMS tests (tenant ordering dependency)
- Keep single worker

---

## Phase 4 — CI Workflow Updates

### 4.1 Workflows to Update

These 5 workflows currently run Cypress tests and must be updated:

1. `.github/workflows/pr_ci.yml`
2. `.github/workflows/main_ci.yml`
3. `.github/workflows/preview_ci.yml`
4. `.github/workflows/release_ci.yml`
5. `.github/workflows/functional_all_db.yml`

### 4.2 Changes for `pr_ci.yml`, `main_ci.yml`, `preview_ci.yml`, `release_ci.yml`

Replace the Cypress functional test steps:

**Before (Cypress):**
```yaml
- name: Functional Tests
  if: matrix.os == 'ubuntu-24.04'
  run: |
    cd test/OrchardCore.Tests.Functional
    npm install
    npm run cms:test
    npm run mvc:test
```

**After (Playwright):**
```yaml
- name: Install Playwright Browsers
  if: matrix.os == 'ubuntu-24.04'
  run: |
    cd test/OrchardCore.Tests.Playwright
    npm ci
    npx playwright install chromium --with-deps

- name: Functional Tests (CMS)
  if: matrix.os == 'ubuntu-24.04'
  run: |
    cd test/OrchardCore.Tests.Playwright
    npm run test:cms

- name: Functional Tests (MVC)
  if: matrix.os == 'ubuntu-24.04'
  run: |
    cd test/OrchardCore.Tests.Playwright
    npm run test:mvc
```

**Update artifact upload paths:**

**Before:**
```yaml
- uses: actions/upload-artifact@v4
  if: failure()
  with:
    name: functional-cms-screenshots
    path: test/OrchardCore.Tests.Functional/cms-tests/cypress/screenshots
```

**After:**
```yaml
- uses: actions/upload-artifact@v4
  if: failure()
  with:
    name: functional-playwright-report
    path: test/OrchardCore.Tests.Playwright/playwright-report/

- uses: actions/upload-artifact@v4
  if: failure()
  with:
    name: functional-playwright-results
    path: test/OrchardCore.Tests.Playwright/test-results/
```

### 4.3 Changes for `functional_all_db.yml`

This workflow currently uses the `cypress/included:15.11.0` Docker container. Replace with the official Playwright container or install Playwright in the Ubuntu runner.

**Option A — Playwright Docker image (recommended):**
```yaml
container:
  image: mcr.microsoft.com/playwright:v1.50.0-noble
```

**Option B — Install on runner:**
```yaml
- name: Install Playwright
  run: |
    cd test/OrchardCore.Tests.Playwright
    npm ci
    npx playwright install chromium --with-deps
```

**Update database-specific jobs** to pass environment variables for the database provider and connection string (same env vars as before — `OrchardCore__ConnectionString`, `OrchardCore__DatabaseProvider`, etc.).

### 4.4 Node.js Version

The current workflows use Node 15 (`node-version: '15'`). Playwright requires Node 18+. Update:

```yaml
- uses: actions/setup-node@v6.3.0
  with:
    node-version: '20'
```

### 4.5 Log Artifact Updates

Keep the application log upload step unchanged — it already points to `src/OrchardCore.Cms.Web/App_Data/logs`.

---

## Phase 5 — Cleanup

After the migration is complete and CI is green:

1. **Remove** the `test/OrchardCore.Tests.Functional/` directory entirely.
2. **Remove** the `cypress-orchardcore` npm package references if no external consumers depend on it.
3. **Remove** Cypress-related dependencies from root `package.json` (if any).
4. **Update** contributing docs and README references to point to the Playwright tests.

---

## Command Mapping Reference

Quick reference for converting Cypress idioms to Playwright:

| Cypress | Playwright |
|---|---|
| `cy.visit(url)` | `await page.goto(url)` |
| `cy.get(selector)` | `page.locator(selector)` |
| `cy.get(sel).click()` | `await page.locator(sel).click()` |
| `cy.get(sel).type(text)` | `await page.locator(sel).fill(text)` |
| `cy.contains(text)` | `page.getByText(text)` |
| `cy.get(sel).should('be.visible')` | `await expect(page.locator(sel)).toBeVisible()` |
| `cy.get(sel).should('contain', t)` | `await expect(page.locator(sel)).toContainText(t)` |
| `cy.url().should('contain', x)` | `await expect(page).toHaveURL(new RegExp(x))` |
| `cy.wait(ms)` | `await page.waitForTimeout(ms)` (avoid if possible) |
| `cy.request(...)` | `await page.request.get(...)` or `await request.post(...)` |
| `cy.fixture(path)` | `fs.readFileSync(path)` or Playwright fixture mechanism |
| `cy.intercept(...)` | `await page.route(...)` |
| `Cypress.env(key)` | `process.env[key]` or Playwright config `use.baseURL` |
| `cy.get('[data-cy=x]')` | `page.locator('[data-cy=x]')` |

---

## Multi-Database Support

The `functional_all_db.yml` workflow tests against SQLite, PostgreSQL, MySQL, and MSSQL. This is configured entirely via environment variables — no test code changes needed. The same Playwright tests run against all databases; only the CI environment variables change:

| Database | `OrchardCore__DatabaseProvider` | Connection String |
|---|---|---|
| SQLite | _(default, no env var)_ | _(default)_ |
| PostgreSQL | `Postgres` | `User ID=postgres;Password=admin;Host=postgres;Port=5432;Database=app;` |
| MySQL | `MySql` | `server=mysql;uid=root;pwd=test123;database=test` |
| MSSQL | `SqlConnection` | `Server=mssql;Database=tempdb;User Id=sa;Password=Password12!;Encrypt=False` |

All database jobs should also set `OrchardCore__OrchardCore_YesSql__EnableThreadSafetyChecks=true`.

---

## Implementation Order

1. **Phase 1** — Create helper modules (can be done incrementally)
2. **Phase 2** — Migrate tests one-by-one, validating each locally
3. **Phase 3** — Implement app lifecycle management (global setup/teardown)
4. **Phase 2+3 validation** — Run all migrated tests locally end-to-end
5. **Phase 4** — Update CI workflows (do this in a single PR with the test migration)
6. **Phase 5** — Remove Cypress after CI is confirmed green
