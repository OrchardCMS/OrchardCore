const puppeteer = require('puppeteer');
const orchard = require('./orchard.js');

let browser;
let page;
let basePath;
let error;

// e.g., npm test --debug
// In debug mode we show the editor, slow down operations, and increase the timeout for each test
let debug = process.env.npm_config_debug || false;
jest.setTimeout(debug ? 60000 : 30000);

beforeAll(async () => {
    try {
        basePath = orchard.run('../../src/OrchardCore.Cms.Web', 'OrchardCore.Cms.Web.dll');
        browser = await puppeteer.launch(debug ? { headless: false, slowMo: 100 } : {});
        page = await browser.newPage();
    } catch (ex) {
        error = ex;
    }
});

afterAll(async () => {
    if (browser) {
        await browser.close();
    }

    orchard.stop();
});

describe('Browser is initialized', () => {
    // Workaround for https://github.com/jasmine/jasmine/issues/1533.
    // Jasmine will not report errors from beforeAll and instead continue running tests which will
    // inevitably fail since the initial state isn't correct.
    // This test allows us to print the error from beforeAll, if any.
    test('no errors on launch', () => {
        expect(error).toBeUndefined();

        // Sanity testing
        expect(basePath).toBeDefined();
        expect(browser).toBeDefined();
        expect(page).toBeDefined();
    })
})

describe('Setup', () => {

    beforeAll(async () => {
        await page.goto(`${basePath}`);
    });

    it('should display "Orchard Setup"', async () => {
        await expect(await page.content()).toMatch('Orchard Setup');
    });

    it('should focus on the site name', async () => {
        await expect(await page.evaluate(() => document.activeElement.id)).toMatch('SiteName');
    });

    it('should not be able to submit the form', async () => {
        // Same as page.keyboard.press('Enter');
        await page.click('#SubmitButton');
        await expect(await page.evaluate(() => document.activeElement.id)).toMatch('SiteName');
    });

    it('should setup a SaaS site on sqlite', async () => {
        await page.type('#SiteName', 'My Sites');
        await page.click('#recipeButton');
        await (await page.$x("//a[contains(text(), 'Software as a Service')]"))[0].click();
        await page.select('#DatabaseProvider', 'Sqlite');
        await page.type('#TablePrefix', '');
        await page.type('#UserName', 'admin');
        await page.type('#Email', 'admin@orchard.com');
        await page.type('#Password', 'Demo123!');
        await page.type('#PasswordConfirmation', 'Demo123!');

        await Promise.all([
            page.waitForNavigation(),
            page.click('#SubmitButton')
        ]);

        await expect(await page.content()).toMatch('Welcome to the Orchard Framework, your site has been successfully set up');
    });
});

describe('Create Tenants', () => {

    it('should display login form', async () => {
        await page.goto(`${basePath}/Login`);
        await expect(await page.content()).toMatch('Use a local account to log in');
    });

    it('should login with setup credentials', async () => {
        await page.type('#UserName', 'admin');
        await page.type('#Password', 'Demo123!');
        await Promise.all([
            page.waitForNavigation(),
            page.keyboard.press('Enter')
        ]);
        
        await expect(await page.url()).toBe(`${basePath}/`);
        await expect(await page.content()).toMatch('admin');
    });

    it('should display a single tenant', async () => {
        await page.goto(`${basePath}/OrchardCore.Tenants/Admin/Index`);
        await expect(await page.content()).toMatch('Default');

        await expect((await page.$$("div.properties")).length).toBe(1);
    });

    it('should create a tenant based on Agency', async () => {

        // Create tenant
        await page.goto(`${basePath}/OrchardCore.Tenants/Admin/Create`);
        
        await page.type('#Name', 'Agency');
        await page.type('#RequestUrlPrefix', 'agency');

        await Promise.all([
            page.waitForNavigation(),
            (await page.$x("//button[contains(text(), 'Create')]"))[0].click()
        ]);

        await expect(await page.url()).toBe(`${basePath}/OrchardCore.Tenants/Admin/Index`);
        await expect(await page.content()).toMatch('Agency');

        // Go to Setup page
        await page.click('#btn-setup-Agency');

        await expect(await page.content()).toMatch('Orchard Setup');

        // Setup site
        await page.type('#SiteName', 'Agency');
        await page.click('#recipeButton');
        await (await page.$x("//a[contains(text(), 'Agency')]"))[0].click();
        await page.select('#DatabaseProvider', 'Sqlite');
        await page.type('#TablePrefix', '');
        await page.type('#UserName', 'admin');
        await page.type('#Email', 'admin@orchard.com');
        await page.type('#Password', 'Demo123!');
        await page.type('#PasswordConfirmation', 'Demo123!');

        await Promise.all([
            page.waitForNavigation(),
            page.click('#SubmitButton')
        ]);
        
        await expect(await page.content()).toMatch('Lorem ipsum dolor sit amet consectetur');
    });

    it('should create a tenant based on Blog', async () => {

        // Create tenant
        await page.goto(`${basePath}/OrchardCore.Tenants/Admin/Create`);
        
        await page.type('#Name', 'Blog');
        await page.type('#RequestUrlPrefix', 'blog');

        await Promise.all([
            page.waitForNavigation(),
            (await page.$x("//button[contains(text(), 'Create')]"))[0].click()
        ]);

        await expect(await page.url()).toBe(`${basePath}/OrchardCore.Tenants/Admin/Index`);
        await expect(await page.content()).toMatch('Blog');

        // Go to Setup page
        await page.click('#btn-setup-Blog');

        await expect(await page.content()).toMatch('Orchard Setup');

        // Setup site
        await page.type('#SiteName', 'Blog');
        await page.click('#recipeButton');
        await (await page.$x("//a[contains(text(), 'Blog')]"))[0].click();
        await page.select('#DatabaseProvider', 'Sqlite');
        await page.type('#TablePrefix', '');
        await page.type('#UserName', 'admin');
        await page.type('#Email', 'admin@orchard.com');
        await page.type('#Password', 'Demo123!');
        await page.type('#PasswordConfirmation', 'Demo123!');

        await Promise.all([
            page.waitForNavigation(),
            page.click('#SubmitButton')
        ]);
        
        await expect(await page.content()).toMatch('This is the description of your blog');
    });
});
