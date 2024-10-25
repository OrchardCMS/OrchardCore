#!/usr/bin/env node

'use strict';

const glob = require("glob"),
      exec = require('child_process').exec,
      packageFileName = 'package.json';

const assetPaths = glob.sync("./src/OrchardCore.{Modules,Themes}/*/" + packageFileName, {});

assetPaths.forEach(function (assetPath) {
    let path = assetPath.substring(0, assetPath.length - packageFileName.length);
    console.log(`Running 'npm update' on '${assetPath}'`);
    exec('npm update', {
        'cwd': path
    }, (error, stdout, stderr) => {
        if (error) {
            console.log(`Failed to run 'npm update' on '${assetPath}'`, error);
        }
    })
});
