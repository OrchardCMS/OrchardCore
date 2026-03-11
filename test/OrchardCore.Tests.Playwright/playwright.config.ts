import { defineConfig } from '@playwright/test';

export default defineConfig({
    testDir: './tests',
    fullyParallel: false,
    forbidOnly: !!process.env.CI,
    retries: process.env.CI ? 2 : 0,
    workers: 1,
    reporter: 'html',
    timeout: 120_000,
    use: {
        baseURL: process.env.ORCHARD_URL || 'http://localhost:5000',
        screenshot: 'only-on-failure',
        video: 'on-first-retry',
        trace: 'on-first-retry',
        browserName: 'chromium',
    },
    globalSetup: './global-setup.ts',
    globalTeardown: './global-teardown.ts',
    projects: [
        {
            name: 'saas-setup',
            testMatch: 'tests/cms/saas-setup.spec.ts',
        },
        {
            name: 'cms',
            testMatch: 'tests/cms/!(saas-setup).spec.ts',
            dependencies: ['saas-setup'],
        },
        {
            name: 'mvc',
            testMatch: 'tests/mvc/**/*.spec.ts',
        },
    ],
});
