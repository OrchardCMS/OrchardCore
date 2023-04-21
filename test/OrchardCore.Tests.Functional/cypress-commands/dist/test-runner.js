'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

// This module was originally build by the OrchardCore team
const child_process = require("child_process");
const fs = require("fs-extra");
const path = require("path");

global.log = function(msg) {
  let now = new Date().toLocaleTimeString();
  console.log(`[${now}] ${msg}\n`);
};

// Build the dotnet application in release mode
function build(dir, dotnetVersion) {
  global.log("version: 3");

  // "dotnet" command arguments.
  let runArgs = [
      'build',
      '--configuration', 'Release',
      '--framework', dotnetVersion
  ];

  // "dotnet" process options.
  let runOpts = {
      cwd: dir
  };

  try {
    var result = child_process.spawnSync("dotnet", ["build", "--help"], { cwd: dir });
    global.log("Testing ...");
    global.log(result.stdout);

    // Run dotnet build process, blocks until process completes.
    let { status, error, stderr, stdout } = child_process.spawnSync('dotnet', runArgs, runOpts);

    if (error) {
      throw error;
    }

    if (status !== 0) {
      if (stderr.length > 0) {
        throw new Error(stderr.toString());
      }
      throw new Error(stdout.toString());
    }

    console.log(stdout.toString());
    console.log('Build successful.');
  }
  catch (error) {
    console.error(error);
    console.error('Failed to build.');
  }
}

// destructive action that deletes the App_Data folder
function deleteDirectory(dir) {
  fs.removeSync(dir);
  global.log(`${dir} deleted`);
}

// Host the dotnet application, does not rebuild
function host(dir, assembly, { appDataLocation='./App_Data', dotnetVersion='net7.0' }={}) {
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
    { cwd: dir, env: {...process.env, ...ocEnv} }
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
function e2e(dir, assembly, { dotnetVersion='net7.0' }={}) {
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
