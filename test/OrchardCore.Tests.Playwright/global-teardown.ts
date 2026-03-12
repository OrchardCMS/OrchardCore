import path from 'path';
import fs from 'fs';
import { deleteMigrationsRecipe, deleteRecipe } from './helpers/app-lifecycle';

const isMvc = process.env.ORCHARD_APP === 'mvc';
const projectRoot = path.resolve(__dirname, '..', '..');
const cmsAppDir = path.join(projectRoot, 'src', 'OrchardCore.Cms.Web');

async function globalTeardown(): Promise<void> {
    if (process.env.ORCHARD_EXTERNAL) {
        return;
    }

    const pidFile = path.join(__dirname, '.server-pid');
    if (fs.existsSync(pidFile)) {
        const pid = parseInt(fs.readFileSync(pidFile, 'utf-8'), 10);
        try {
            process.kill(pid, 'SIGINT');
            console.log(`Server process ${pid} killed.`);
        } catch {
            console.log(`Server process ${pid} already exited.`);
        }
        fs.unlinkSync(pidFile);
    }

    // Clean up test recipes copied during global setup.
    if (!isMvc) {
        deleteMigrationsRecipe(cmsAppDir);
        deleteRecipe(cmsAppDir, 'media-tus.recipe.json');
    }
}

export default globalTeardown;
