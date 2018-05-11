const puppeteer = require('puppeteer');
const { spawn } = require('child_process');
const rimraf = require('rimraf');

let browser;
let page;
let server;
const basePath = "http://localhost:5000";

// e.g., npm test --debug
// In debug mode we show the editor, slow down operations, and increase the timeout for each test
let debug = process.env.npm_config_debug || false;
jest.setTimeout(debug ? 60000 : 15000);

beforeAll(async () => {

    console.log('Deleting App_Data ...')
    rimraf.sync('./publish/App_Data', [], function () { console.log('App_Data deleted'); });

    console.log('Starting application ...')
    server = spawn('dotnet', ['OrchardCore.Cms.Web.dll'], { cwd: './publish' });

    server.stdout.on('data', (data) => {
        let now = new Date().toLocaleTimeString();
        console.log(`[${now}] ${data}`);
    });
      
    server.stderr.on('data', (data) => {
        console.log(`stderr: ${data}`);
    });
      
    server.on('close', (code) => {
        console.log(`Server process exited with code ${code}`);
    });

    browser = await puppeteer.launch(debug ? { headless: false, slowMo: 100  } : { });
    page = await browser.newPage();
});

afterAll(async () => {
    await browser.close();
    server.kill();
});

describe('Setup', () => {

    beforeAll(async () => {
        await page.goto(`${basePath}`);
    });

    it('should display "Orchard Setup"', async () => {
        await expect(await page.content()).toMatch('Orchard Setup')
    });

    it('should focus on the site name', async () => {
        await expect(await page.evaluate(() => document.activeElement.id)).toMatch('SiteName')
    });

    it('should not be able to submit the form', async () => {
        // Same as page.keyboard.press('Enter');
        await page.click('#SubmitButton')
        await expect(await page.evaluate(() => document.activeElement.id)).toMatch('SiteName')
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
        
        await expect(await page.content()).toMatch('Welcome to the Orchard Framework, your site has been successfully set up 1')
    });
})

// describe('Create Tenants', () => {

//     it('should display login form', async () => {
//         await page.goto(`${basePath}/Login`);
//         await expect(await page.content()).toMatch('Use a local account to log in')
//     });

//     it('should login with setup credentials', async () => {
//         await page.type('#UserName', 'admin');
//         await page.type('#Password', 'Demo123!');
//         await Promise.all([
//             page.waitForNavigation(),
//             page.keyboard.press('Enter')
//         ]);
        
//         await expect(await page.url()).toBe(`${basePath}/`);
//         await expect(await page.content()).toMatch('admin');
//     });

//     it('should display a single tenant', async () => {
//         await page.goto(`${basePath}/OrchardCore.Tenants/Admin/Index`);
//         await expect(await page.content()).toMatch('Default')

//         var tenantsCount = expect((await page.$$("div.properties")).length).toBe(1);
//     });

//     it('should create a tenant based on Agency', async () => {

//         // Create tenant
//         await page.goto(`${basePath}/OrchardCore.Tenants/Admin/Create`);
        
//         await page.type('#Name', 'Agency');
//         await page.type('#RequestUrlPrefix', 'agency');

//         await Promise.all([
//             page.waitForNavigation(),
//             (await page.$x("//button[contains(text(), 'Create')]"))[0].click()
//         ]);

//         await expect(await page.url()).toBe(`${basePath}/OrchardCore.Tenants/Admin/Index`);
//         await expect(await page.content()).toMatch('Agency')

//         // Go to Setup page
//         await page.goto(`${basePath}/agency/`);
//         await expect(await page.content()).toMatch('Orchard Setup')

//         // Setup site
//         await page.type('#SiteName', 'Agency');
//         await page.click('#recipeButton');
//         await (await page.$x("//a[contains(text(), 'Agency')]"))[0].click();
//         await page.select('#DatabaseProvider', 'Sqlite');
//         await page.type('#TablePrefix', '');
//         await page.type('#UserName', 'admin');
//         await page.type('#Email', 'admin@orchard.com');
//         await page.type('#Password', 'Demo123!');
//         await page.type('#PasswordConfirmation', 'Demo123!');

//         await Promise.all([
//             page.waitForNavigation(),
//             page.click('#SubmitButton')
//         ]);
        
//         await expect(await page.content()).toMatch('Lorem ipsum dolor sit amet consectetur')
//     });

//     it('should create a tenant based on Blog', async () => {

//         // Create tenant
//         await page.goto(`${basePath}/OrchardCore.Tenants/Admin/Create`);
        
//         await page.type('#Name', 'Blog');
//         await page.type('#RequestUrlPrefix', 'blog');

//         await Promise.all([
//             page.waitForNavigation(),
//             (await page.$x("//button[contains(text(), 'Create')]"))[0].click()
//         ]);

//         await expect(await page.url()).toBe(`${basePath}/OrchardCore.Tenants/Admin/Index`);
//         await expect(await page.content()).toMatch('Blog')

//         // Go to Setup page
//         await page.goto(`${basePath}/blog/`);
//         await expect(await page.content()).toMatch('Orchard Setup')

//         // Setup site
//         await page.type('#SiteName', 'Blog');
//         await page.click('#recipeButton');
//         await (await page.$x("//a[contains(text(), 'Blog')]"))[0].click();
//         await page.select('#DatabaseProvider', 'Sqlite');
//         await page.type('#TablePrefix', '');
//         await page.type('#UserName', 'admin');
//         await page.type('#Email', 'admin@orchard.com');
//         await page.type('#Password', 'Demo123!');
//         await page.type('#PasswordConfirmation', 'Demo123!');

//         await Promise.all([
//             page.waitForNavigation(),
//             page.click('#SubmitButton')
//         ]);
        
//         await expect(await page.content()).toMatch('This is the description of your blog')
//     });
// })
