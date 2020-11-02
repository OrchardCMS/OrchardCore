const sh = require('shelljs');
const path = require('path');

const destPath = path.resolve(path.dirname(__filename), '../dist');

sh.rm('-rf', `${destPath}/*`)

