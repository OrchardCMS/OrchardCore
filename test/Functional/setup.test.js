const puppeteer = require('puppeteer');

let browser;
let page;

beforeAll(async () => {
    browser = await puppeteer.launch();
    page = await browser.newPage();
}, 15000);

afterAll(async () => {
    await browser.close();
});

describe('Setup', () => {

    beforeAll(async () => {
        await page.goto('http://localhost:5000');
    });

    it('should display "Orchard Setup"', async () => {
        await expect(await page.content()).toMatch('Orchard Setup')
    });

    it('should focus on the site name', async () => {
        await expect(await page.evaluate(() => document.activeElement.id)).toMatch('SiteName')
    });

    it('should not be able to submit the form', async () => {
        page.keyboard.press('Enter');
        await expect(await page.evaluate(() => document.activeElement.id)).toMatch('SiteName')
    });

    it('should setup an Agency site on sqlite', async () => {
        await page.type('#SiteName', 'My Agency');
        await page.type('#recipeButton', 'Agency');
        await page.select('#DatabaseProvider', 'Sqlite')
        await page.type('#TablePrefix', '');
        await page.type('#UserName', 'admin');
        await page.type('#Email', 'admin@orchard.com');
        await page.type('#Password', 'Demo123!');
        await page.type('#PasswordConfirmation', 'Demo123!');

        page.keyboard.press('Enter');
        // would be easier if the button had an id
        // await page.click('#SubmitButton')
        
        await page.waitForNavigation();
        await expect(await page.title()).toMatch('My Agency')
    }, 15000);
})
