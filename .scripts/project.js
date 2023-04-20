#!/usr/bin/env node

'use strict';

if(process.argv.length !== 4) {

    console.log('Invalid arguments. This command accepts a single project-name argument.');

    return;
}

const actionName = process.argv[2];

if(actionName != 'install' && actionName != 'update') {

    console.log('Invalid arguments. The first argument should be either "install" or "udpate".');

    return;
}

const projectName = process.argv[3];

const glob = require("glob"),
      exec = require('child_process').exec;

const projects = glob.sync("./src/OrchardCore.{Modules,Themes}/*" + projectName + "*/package.json", {});

if(projects.length == 0) {

    console.log('Unable to find a project with a name that contains the name ' + process.argv[2]);

    return;
}

if(projects.length > 1) {

    console.log('Found multiple projects that contains the name ' + process.argv[2] + '. Please specify a more specific name.');
    console.log('Here is a list of the matches found');
    console.log(projects);

    return;
}

const path = projects[0].substring(0, projects[0].length - 'package.json'.length);

console.log(actionName + 'ing: ' + projects[0]);

exec('npm ' + actionName, {
    'cwd': path
}, (error, stdout, stderr) => {
    console.log(error, stdout, stderr);
});
