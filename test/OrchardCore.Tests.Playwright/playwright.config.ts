import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
    testDir: './tests',
    fullyParallel: false,
    forbidOnly: !!process.env.CI,
    retries: process.env.CI ? 2 : 0,
    workers: 1,
    reporter: 'html',
    timeout: 120_000,
    globalSetup: process.env.ORCHARD_EXTERNAL ? undefined : './global-setup.ts',
    globalTeardown: process.env.ORCHARD_EXTERNAL ? undefined : './global-teardown.ts',
    use: {
        baseURL: process.env.ORCHARD_URL || 'http://localhost:5000',
        trace: 'on-first-retry',
        screenshot: 'only-on-failure',
        video: 'on-first-retry',
    },
    projects: [
        {
            name: 'saas-setup',
            testMatch: /saas-setup\.spec\.ts/,
            use: { ...devices['Desktop Chrome'] },
        },
        {
            name: 'cms',
            testDir: './tests/cms',
            testIgnore: /saas-setup\.spec\.ts/,
            dependencies: ['saas-setup'],
            use: { ...devices['Desktop Chrome'] },
        },
        {
            name: 'mvc',
            testDir: './tests/mvc',
            use: { ...devices['Desktop Chrome'] },
        },
    ],
});
