'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

// This module was originally build by the OrchardCore team
const child_process = require("child_process");
const fs = require("fs");
const path = require("path");
const rimraf = require("rimraf");

global.log = function(msg) {
  let now = new Date().toLocaleTimeString();
  console.log(`[${now}] ${msg}\n`);
};

// Build the dotnet application in release mode
function build(dir) {
  global.log("Building ...");
  child_process.spawnSync("dotnet", ["build", "-c", "Release"], { cwd: dir });
}

// destructive action that deletes the App_Data folder
function clean(dir) {
  rimraf(path.join(dir, "App_Data"), function() {
    global.log("App_Data deleted");
  });
}

// Host the dotnet application, does not rebuild
function host(dir, assembly) {
  if (fs.existsSync(path.join(dir, "bin/Release/netcoreapp3.1/", assembly))) {
    global.log("Application already built, skipping build");
  } else {
    build(dir);
  }
  global.log("Starting application ...");
  let server = child_process.spawn(
    "dotnet",
    ["bin/Release/netcoreapp3.1/" + assembly],
    { cwd: dir }
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
function e2e(dir, assembly, performClean = false, rebuild = false) {
  if (performClean === true) {
    clean(dir);
  }
  if (rebuild === true) {
    build(dir);
  }

  if (fs.existsSync(path.join(dir, "bin/Release/netcoreapp3.1/", assembly))) {
    global.log("Application already built, skipping build");
  } else {
    build(dir);
  }

  var server = host(dir, assembly);

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
exports.clean = clean;
exports.e2e = e2e;
exports.host = host;
