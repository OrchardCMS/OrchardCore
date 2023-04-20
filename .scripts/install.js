#!/usr/bin/env node

'use strict';

var glob = require("glob"),
	exec = require('child_process').exec;

var assetPaths = glob.sync("./src/OrchardCore.{Modules,Themes}/*/package.json", {});

assetPaths.forEach(function (assetPath) {
    var path = assetPath.substring(0, assetPath.length - 12);
    console.log('Installing...' + assetPath);
    exec('npm install', {
        'cwd': path
    }, (error, stdout, stderr) => {
         console.log(error, stdout, stderr);
    })
});
