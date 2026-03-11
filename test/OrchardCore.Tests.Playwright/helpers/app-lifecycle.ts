import { execSync, spawn, type ChildProcess } from 'child_process';
import path from 'path';
import fs from 'fs';

const DOTNET_VERSION = 'net10.0';

function log(msg: string): void {
    const now = new Date().toLocaleTimeString();
    console.log(`[${now}] ${msg}`);
}

export function buildApp(appDir: string): void {
    log('Building application...');
    execSync(`dotnet build -c Release -f ${DOTNET_VERSION}`, { cwd: appDir, stdio: 'inherit' });
    log('Build complete.');
}

export function deleteAppData(appDir: string, dataDir: string = 'App_Data_Tests'): void {
    const fullPath = path.join(appDir, dataDir);
    if (fs.existsSync(fullPath)) {
        fs.rmSync(fullPath, { recursive: true, force: true });
        log(`${fullPath} deleted`);
    }
}

export function copyMigrationsRecipe(sourceDir: string, appDir: string): void {
    const recipeFileName = 'migrations.recipe.json';
    const sourcePath = path.join(sourceDir, recipeFileName);
    const destDir = path.join(appDir, 'Recipes');
    const destPath = path.join(destDir, recipeFileName);

    if (!fs.existsSync(sourcePath) || fs.existsSync(destPath)) {
        return;
    }

    if (!fs.existsSync(destDir)) {
        fs.mkdirSync(destDir, { recursive: true });
    }

    fs.copyFileSync(sourcePath, destPath);
    log(`Migrations recipe copied to ${destDir}`);
}

export function deleteMigrationsRecipe(appDir: string): void {
    const destDir = path.join(appDir, 'Recipes');
    const destPath = path.join(destDir, 'migrations.recipe.json');

    if (fs.existsSync(destPath)) {
        fs.unlinkSync(destPath);
        log(`Migrations recipe deleted from ${destDir}`);
    }

    // Remove Recipes dir if empty.
    if (fs.existsSync(destDir) && fs.readdirSync(destDir).length === 0) {
        fs.rmdirSync(destDir);
    }
}

export function hostApp(appDir: string, assembly: string): ChildProcess {
    const binPath = path.join('bin', 'Release', DOTNET_VERSION, assembly);

    if (!fs.existsSync(path.join(appDir, binPath))) {
        buildApp(appDir);
    }

    log('Starting application...');

    const server = spawn('dotnet', [binPath], {
        cwd: appDir,
        env: {
            ...process.env,
            ORCHARD_APP_DATA: './App_Data_Tests',
        },
    });

    server.stdout?.on('data', (data) => {
        const msg = data.toString();
        if (msg.includes('Exception') || msg.startsWith('fail:')) {
            console.error(`[Server Error] ${msg}`);
        }
    });

    server.stderr?.on('data', (data) => {
        console.error(`[Server stderr] ${data}`);
    });

    server.on('close', (code) => {
        log(`Server process exited with code ${code}`);
    });

    return server;
}

export async function waitForReady(baseUrl: string, timeoutMs: number = 60000): Promise<void> {
    const start = Date.now();
    log(`Waiting for server at ${baseUrl}...`);

    while (Date.now() - start < timeoutMs) {
        try {
            const response = await fetch(baseUrl);
            if (response.ok || response.status === 302 || response.status === 404) {
                log('Server is ready.');
                return;
            }
        } catch {
            // Server not ready yet
        }
        await new Promise((resolve) => setTimeout(resolve, 1000));
    }

    throw new Error(`Server at ${baseUrl} did not become ready within ${timeoutMs}ms`);
}

export function killApp(proc: ChildProcess): void {
    if (proc && !proc.killed) {
        proc.kill('SIGINT');
        log('Server process killed.');
    }
}
