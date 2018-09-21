const puppeteer = require('puppeteer');
const child_process = require('child_process');

let browser;
let page;
let server;
const basePath = "http://localhost:5000";

// e.g., npm test --debug
// In debug mode we show the editor, slow down operations, and increase the timeout for each test
let debug = process.env.npm_config_debug || false;
jest.setTimeout(debug ? 60000 : 30000);

beforeAll(async () => {

    console.log('Building ...');
    child_process.spawnSync('dotnet', ['build', '-c', 'release'], { cwd: '../../src/OrchardCore.Nancy.Web' });

    console.log('Starting application ...');
    server = child_process.spawn('dotnet', ['bin/release/netcoreapp2.1/OrchardCore.Nancy.Web.dll'], { cwd: '../../src/OrchardCore.Nancy.Web' });

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
    if (browser) {
        await browser.close();
    }

    if (server) {
        server.kill('SIGINT');
    }
});

describe('Nancy', () => {

    it('should display "Hello"', async () => {
        await page.goto(`${basePath}`);
        await expect(await page.content()).toMatch('Hello from Nancy');
    });

    it('should display category', async () => {
        await page.goto(`${basePath}/foo`);
        await expect(await page.content()).toMatch('My category is foo');
    });
});
