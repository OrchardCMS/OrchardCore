'use strict';
const path = require('path');
const sh = require('shelljs');
const renderPug = require('./render-pug');

const srcPath = path.resolve(path.dirname(__filename), '../src');

sh.find(srcPath).forEach(_processFile);

function _processFile(filePath) {
    if (
        filePath.match(/\.pug$/)
        && !filePath.match(/include/)
        && !filePath.match(/mixin/)
        && !filePath.match(/\/pug\/layouts\//)
    ) {
        renderPug(filePath);
    }
}