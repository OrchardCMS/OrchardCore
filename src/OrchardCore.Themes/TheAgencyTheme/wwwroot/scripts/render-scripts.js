'use strict';
const fs = require('fs');
const packageJSON = require('../package.json');
const path = require('path');
const sh = require('shelljs');

module.exports = function renderScripts() {
    const sourcePath = path.resolve(path.dirname(__filename), '../src/js/scripts.js');
    const destPath = path.resolve(path.dirname(__filename), '../dist/js/scripts.js');
    
    const copyright = `/*!
    * Start Bootstrap - ${packageJSON.title} v${packageJSON.version} (${packageJSON.homepage})
    * Copyright 2013-${new Date().getFullYear()} ${packageJSON.author}
    * Licensed under ${packageJSON.license} (https://github.com/BlackrockDigital/${packageJSON.name}/blob/master/LICENSE)
    */
    `
    const scriptsJS = fs.readFileSync(sourcePath);
    const destPathDirname = path.dirname(destPath);
    
    if (!sh.test('-e', destPathDirname)) {
        sh.mkdir('-p', destPathDirname);
    }
    
    fs.writeFileSync(destPath, copyright + scriptsJS);
};