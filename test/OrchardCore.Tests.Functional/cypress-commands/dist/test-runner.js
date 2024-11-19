'use strict';

// This module was originally build by the OrchardCore team
const child_process = require("child_process");
const fs = require("fs-extra");
const path = require("path");

global.log = function (msg) {
    let now = new Date().toLocaleTimeString();
    console.log(`[${now}] ${msg}\n`);

    if (msg.indexOf("Exception") >= 0) {
        throw new Error("An exception was detected");
    }

    if (msg.indexOf("fail:") == 0) {
        throw new Error("An error was logged");
    }
};

// Build the dotnet application in release mode
function build(dir, dotnetVersion) {
    global.log("Building ...");
    child_process.spawnSync("dotnet", ["build", "-c", "Release", "-f", dotnetVersion], { cwd: dir });
}

// destructive action that deletes the App_Data folder
function deleteDirectory(dir) {
    fs.removeSync(dir);
    global.log(`${dir} deleted`);
}

// Copy the migrations recipe.
function copyMigrationsRecipeFile(dir) {

    const recipeFilePath = 'Recipes/migrations.recipe.json';

    if (!fs.existsSync(`./${recipeFilePath}`) || fs.existsSync(`${dir}/${recipeFilePath}`)) {
        return;
    }

    if (!fs.existsSync(`${dir}/Recipes`)) {
        fs.mkdirSync(`${dir}/Recipes`);
    }

    fs.copyFile(`./${recipeFilePath}`, `${dir}/${recipeFilePath}`);
    global.log(`migrations recipe copied to ${dir}/Recipes`);
}

// Host the dotnet application, does not rebuild
function host(dir, assembly, { appDataLocation = './App_Data', dotnetVersion = 'net8.0' } = {}) {
    if (fs.existsSync(path.join(dir, `bin/Release/${dotnetVersion}/`, assembly))) {
        global.log("Application already built, skipping build");
    } else {
        build(dir, dotnetVersion);
    }
    global.log("Starting application ...");

    const ocEnv = {};
    ocEnv["ORCHARD_APP_DATA"] = appDataLocation;

    let server = child_process.spawn(
        "dotnet",
        [`bin/Release/${dotnetVersion}/` + assembly],
        { cwd: dir, env: { ...process.env, ...ocEnv } }
    );

    server.stdout.on("data", data => {
        global.log(data);
    });

    server.stderr.on("data", data => {
        global.log(`stderr: ${data}`);
    });

    server.on("close", code => {
        global.log(`Server process exited with code ${code}`);
    });
    return server;
}

// combines the functions above, useful when triggering tests from CI
function e2e(dir, assembly, { dotnetVersion = 'net8.0' } = {}) {
    copyMigrationsRecipeFile(dir);
    deleteDirectory(path.join(dir, "App_Data_Tests"));
    var server = host(dir, assembly, { appDataLocation: "./App_Data_Tests", dotnetVersion });

    let test = child_process.exec("npx cypress run");
    test.stdout.on("data", data => {
        console.log(data);
    });

    test.stderr.on("data", data => {
        console.log(`stderr: ${data}`);
    });

    test.on("close", code => {
        console.log(`Cypress process exited with code ${code}`);
        server.kill("SIGINT");
        process.exit(code);
    });
}

exports.build = build;
exports.deleteDirectory = deleteDirectory;
exports.e2e = e2e;
exports.host = host;
