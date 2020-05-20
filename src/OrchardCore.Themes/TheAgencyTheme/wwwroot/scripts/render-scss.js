'use strict';
const autoprefixer = require('autoprefixer')
const fs = require('fs');
const packageJSON = require('../package.json');
const path = require('path');
const postcss = require('postcss')
const sass = require('sass');
const sh = require('shelljs');

const stylesPath = '../src/scss/styles.scss';
const destPath = path.resolve(path.dirname(__filename), '../dist/css/styles.css');

module.exports = function renderSCSS() {
    
    const results = sass.renderSync({
        data: entryPoint,
        includePaths: [
            path.resolve(path.dirname(__filename), '../node_modules')
        ],
      });

    const destPathDirname = path.dirname(destPath);
    if (!sh.test('-e', destPathDirname)) {
        sh.mkdir('-p', destPathDirname);
    }

    postcss([ autoprefixer ]).process(results.css, {from: 'styles.css', to: 'styles.css'}).then(result => {
        result.warnings().forEach(warn => {
            console.warn(warn.toString())
        })
        fs.writeFileSync(destPath, result.css.toString());
    })

};

const entryPoint = `/*!
* Start Bootstrap - ${packageJSON.title} v${packageJSON.version} (${packageJSON.homepage})
* Copyright 2013-${new Date().getFullYear()} ${packageJSON.author}
* Licensed under ${packageJSON.license} (https://github.com/BlackrockDigital/${packageJSON.name}/blob/master/LICENSE)
*/
@import "${stylesPath}"
`