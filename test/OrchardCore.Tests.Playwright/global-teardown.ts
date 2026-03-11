import path from 'path';
import fs from 'fs';

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
}

export default globalTeardown;
