#!/usr/bin/env node

'use strict';

const glob = require("glob"),
      exec = require('child_process').exec;

const assetPaths = glob.sync("./src/OrchardCore.{Modules,Themes}/*/package.json", {});

assetPaths.forEach(function (assetPath) {
    let path = assetPath.substring(0, assetPath.length - 12);
    console.log('Updating: ' + assetPath);
    exec('npm update', {
        'cwd': path
    }, (error, stdout, stderr) => {
         console.log(error, stdout, stderr);
    })
});
