const child_process = require('child_process');
const fs = require('fs');
const rimraf = require('rimraf');

module.exports = {
    run: function (dir, assembly, clean) {

        if (fs.existsSync(dir + 'bin/release/netcoreapp2.1/' + assembly)) {
            console.log('Application already built, skipping build');
        }
        else {
            console.log('Building ...');
            child_process.spawnSync('dotnet', ['build', '-c', 'release'], { cwd: dir });
        }

        if (clean === true) {
            rimraf(dir + 'App_Data', function () { console.log('App_Data deleted'); });
        }

        console.log('Starting application ...');
        let server = child_process.spawn('dotnet', ['bin/release/netcoreapp2.1/' + assembly], { cwd: dir });

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

        global.__SERVER_GLOBAL__ = server;

        return "http://localhost:5000";
    },
    stop: function () {
        let server = global.__SERVER_GLOBAL__;

        if (server) {
            server.kill('SIGINT');
        }
    }
};