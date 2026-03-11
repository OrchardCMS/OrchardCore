import path from 'path';
import fs from 'fs';
import { deleteAppData, copyMigrationsRecipe, hostApp, waitForReady } from './helpers/app-lifecycle';

const isMvc = process.env.ORCHARD_APP === 'mvc';
const projectRoot = path.resolve(__dirname, '..', '..');

const cmsAppDir = path.join(projectRoot, 'src', 'OrchardCore.Cms.Web');
const mvcAppDir = path.join(projectRoot, 'src', 'OrchardCore.Mvc.Web');
const fixturesDir = path.join(__dirname, 'fixtures');

const appDir = isMvc ? mvcAppDir : cmsAppDir;
const assembly = isMvc ? 'OrchardCore.Mvc.Web.dll' : 'OrchardCore.Cms.Web.dll';
const baseUrl = process.env.ORCHARD_URL || 'http://localhost:5000';

async function globalSetup(): Promise<void> {
    // Skip app lifecycle if ORCHARD_EXTERNAL is set (server managed externally)
    if (process.env.ORCHARD_EXTERNAL) {
        console.log('ORCHARD_EXTERNAL is set, skipping app build/host.');
        return;
    }

    deleteAppData(appDir);

    // Copy migrations recipe for CMS tests (after clean so it's not deleted)
    if (!isMvc) {
        copyMigrationsRecipe(fixturesDir, appDir);
    }

    const server = hostApp(appDir, assembly);

    // Store server PID so global-teardown can kill it
    const pidFile = path.join(__dirname, '.server-pid');
    fs.writeFileSync(pidFile, String(server.pid));

    await waitForReady(baseUrl, 30_000);
}

export default globalSetup;
