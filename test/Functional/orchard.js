const child_process = require('child_process');
const fs = require('fs');
const rimraf = require('rimraf');

var LOG;

module.exports = {
    printLog: function () {
        console.log(LOG);
    },
    log: function (msg) {
        let now = new Date().toLocaleTimeString();
        LOG += `[${now}] ${msg}\n`;
    },
    run: function (dir, assembly, clean) {
        LOG = "";

        if (fs.existsSync(dir + 'bin/release/netcoreapp2.2/' + assembly)) {
            log('Application already built, skipping build');
        }
        else {
            log('Building ...');
            child_process.spawnSync('dotnet', ['build', '-c', 'release'], { cwd: dir });
        }

        if (clean === true) {
            rimraf(dir + 'App_Data', function () { log('App_Data deleted'); });
        }

        log('Starting application ...');
        let server = child_process.spawn('dotnet', ['bin/release/netcoreapp2.2/' + assembly], { cwd: dir });

        server.stdout.on('data', (data) => {
            log(data);
        });

        server.stderr.on('data', (data) => {
            log(`stderr: ${data}`);
        });

        server.on('close', (code) => {
            log(`Server process exited with code ${code}`);
        });

        global.__SERVER_GLOBAL__ = server;

        return "http://localhost:5000";
    },
    stop: function () {
        let server = global.__SERVER_GLOBAL__;

        if (server) {
            server.kill('SIGINT');
        }
    },
    cleanAppData: function (dir) {
        rimraf(dir + 'App_Data', function () { log('App_Data deleted'); });
    }
};