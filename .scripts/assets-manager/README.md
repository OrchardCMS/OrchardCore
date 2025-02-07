# Orchard Core Assets Management

Created originally by @jptissot  
Contributed by @skrypt

Based on [Concurrently](https://github.com/open-cli-tools/concurrently) the Orchard Core assets management tool is used for building, watching, hosting assets. It allows to use bundlers as [Parcel](https://parceljs.org/) [Vite](https://vite.dev/) and [Webpack](https://webpack.js.org/). This is a non-opiniated tool which allows to be extended for any asset compiler/bundler that someone may require in the future.

`Concurrently`, is a concurrent shell runner which allows to trigger any possible shell command. Everything is written as ES6 modules (.mjs files).

Old assets are not compiled as ES6 modules so they don't need these bundlers. For that matter we kept the old gulpfile.js which will be now triggered by `Concurrently` when doing `yarn build -gr`.

Concurrently uses an `Assets.json` file that defines actions to execute.

Gulp uses a GulpAssets.json file that defines actions to execute.

## Requirements

Nodejs v22.12.0  
Yarn 4.6.0  
Corepack https://nodejs.org/api/corepack.html  

## Getting started

Clone Orchard Core repository.
From the root folder of that repository execute:

```cmd
corepack enable
yarn
yarn build
```

## Features

- Build everything: `yarn build`
- Build module by name: `yarn build -n module-name`
- Watch module by name: `yarn watch -n module-name`.
- Build with gulp: `yarn build -g`
- Rebuild with gulp: `yarn build -gr`
- Host with dev server: `yarn host -n module-name`.
- Clean folders with `yarn clean`. Will also clean parcel-cache folder.
- Makes uses of latest yarn version 4.6.0. OC is currently on version 1.something
- Makes use of yarn workspaces which allows to import files from different locations in the app for sharing ES6 modules.
- VS Code launcher debug option added as "Asset Bundler Tool Debug"
- Build assets by tag: `yarn build -t tagname`
- Gulp pipeline still using GulpAssets.json file
- New Assets.json file definitions for building with new tool.
- Concurrently will retry building up to 3 times making CI build less prone to fail.
- Moving package.json files to Assets folder of modules and themes. This will remove these files from the compiled C# .dll files.
- Removes unnecessary package-lock.json files as we now rely on a single yarn.lock file.

## Random Notes

- The copy task does not watch for file changes and the watch commands do not copy files. The build command will copy files.

- Use dry-run to see a log of which files are being copied and which asset files we have in the solution.

- Parcel seems a bit picky with transpiling some libraries(azure-maps-control for example)  

## Supported actions

### Parcel 

Runs the Parcel bundler.

**Note**: Sometimes, Parcel is too aggressive in its caching. If you manually delete any output folders, you may need to delete the `.parcel-cache` folder as well for Parcel to write to it again. Running the `yarn clean` command will clean it up for you.

```json

[

  {

    "action": "parcel",

    "name": "module-microsoft-datasource-wrapper",

    "source": "Assets/Scripts/datasource-wrapper.js",

    "dest": "wwwroot/datasource-wrapper",

    "tags":["vue3"]

  }

]

```


The `source` property must be the entry point passed to the parcel bundler.

The `dest` property must be a folder. This is because parcel will usually output multiple files.

The `tags` property can be a string or an array of strings.

You can also pass an `options` object that override parcel options.  

### Vite

Vite bundler action will support any configuration. From bundling a vue app to compiling a simple library. It is working by configuration file. The asset management tool simply loads a vite.config.ts file based on the source folder that we instruct it to use from the Assets.json file.

Example of Assets.json config file:

```json
[
  {
    "action": "vite",
    "name": "my-vue-app",
    "source": "Assets/vite/",
    "tags": ["admin", "dashboard", "js"]
  }
]
```

### Webpack

Webpack bundler action will support any configuration. From bundling a vue app to compiling a simple library. It is working by configuration file. The asset management tool simply loads a given webpack.config.js file that we instruct to use from the Assets.json file.

Example of Assets.json config file:

```json
[
  {
    "action": "webpack",
    "name": "graphiql",
    "config": "/Assets/webpack.config.js",
    "tags": ["admin", "dashboard", "js"]
  }
]
```

### Run

Allows to run a command through Concurrently.

```json

[

  {

    "action": "run",

    "name": "itwin-viewer-app",

    "source": "Assets/itwin-viewer-app",

    "scripts": {

      "build": "yarn run build",

      "watch": "yarn run start"

    }

  }

]

```

Here, the `source` property must be a folder and is used as the working directory where the command is ran.

The scripts keys must match the command used to start the pipeline. If you start the pipeline with `yarn build` this would run all `build` scripts of the run actions.

### Copy

Allows to copy files.

```json

[

  {

    "action": "copy",

    "dryRun": true,

    "name": "copy-bootstrap-js",

    "source": "node_modules/bootstrap/dist/js/*.js*",

    "dest": "wwwroot/Scripts"

  },

  {

    "action": "copy",

    "name": "copy-bootstrap-css",

    "source": "node_modules/bootstrap/dist/css/*.css*",

    "dest": "wwwroot/Styles"

  }

]

```

The source field can be a file, or a glob of files.

The destination should always be a folder as we do not support renaming files.

You can use the dry-run task to log to the console where the files will be copied to.

### Min  

Allows to minify files.

```json

[

  {

    "action": "min",

    "dryRun": true,

    "name": "copy-bootstrap-js",

    "source": "node_modules/bootstrap/dist/js/*.js*",

    "dest": "wwwroot/Scripts"

  },

  {

    "action": "min",

    "name": "copy-bootstrap-css",

    "source": "node_modules/bootstrap/dist/css/*.css*",

    "dest": "wwwroot/Styles"

  }

]

```

The source field can be a file, or a glob of files.

The destination should always be a folder as we do not support renaming files.

You can use the dry-run task to log to the console where the files will be copied to.

### Sass

Allows to transpile scss files.

```json

[

  {

    "action": "sass",

    "name": "transpile-bootstrap-scss",

    "source": "node_modules/bootstrap/dist/css/main.scss",

    "dest": "wwwroot/Styles"

  }

]

```

The source field can be a file, or a glob of files.

The destination should always be a folder as we do not support renaming files.

You can use the dry-run task to log to the console where the files will be copied to.
## build.config.mjs

You can create a `build.config.mjs` file next to the root `package.json`.

This file allows you to customize options used by the build tools.

For example, if you wanted to override the parcel browserlist:

```javascript

// The type of command running and the current group's json object.

export function parcel(type, group) {

  return {

    defaultTargetOptions: {

      engines: {

        browsers: "> 1%, last 4 versions, not dead",

      },

    },

  };

}

```

Here is an example for vite:

```javascript

import vue from "@vitejs/plugin-vue";

export function viteConfig(action) {

  return {

    plugins: [vue()],

    build: {

      minify: false,

      rollupOptions: {

        output: {

          manualChunks: (id) => {

            if (id.includes("node_modules")) {

              if (id.includes("@vue") || id.includes("/vue/")) {

                return "vue";

              }

              return "vendor"; // all other package goes here

            }

          },

        },

      },

    },

  };

}

```

You can also specify the glob pattern used to harvest the `Assets.json` files in your solution in the build.config.mjs file

```javascript

export const assetsLookupGlob = "src/{OrchardCore.Modules,OrchardCore.Themes}/*/Assets.json";

```

## ECMAScript vs CommonJS (Parcel and Vite)

The ECMAScript module (ESM) format is the standardized way of loading JavaScript packages. CommonJS is a legacy implementation of modules that is not standardized. ESM is asynchronously loaded, while CommonJS is synchronous. We should favorize building as ESM modules. 

Vite ([rollup.js](https://rollupjs.org/)) will build by default as ECMAScripts and it is by design. Parcel will automatically build as CommonJS or ECMAScript based on package.json configuration; CommonJS being the default when the "type" parameter is not specified. 

To be able to compile as ECMAScript there are requirements.  

1 - the package.json file needs to have:  

```json

{

  "type": "module",

}

```

This should be enough for any single script files that we want to execute asynchronously. 

Though, for Vue 3 apps to use ECMAScript; it needs to use an alias to its ESM bundler version to prevent needing it everywhere in the different components of the app.  

```javascript

import { createApp } from 'vue' //needs an alias to 'vue/dist/vue.esm-bundler.js'

```

Example for an app that will use Vite/TS would be to add this configuration to a vite.config.ts file.

```json

    resolve: {

        alias: {

            'vue': 'vue/dist/vue.esm-bundler.js',

        },

    },

```

Also, now when adding a `<script>` tag to the HTML you will need to use:

```html

<script type="module" src="somepath"></script>

```

`<script>` tags are non-ESM by default; you have to add a `type="module"` attribute to opt into ESM mode.

Meaning that the ESM script will be interpreted as CommonJS by the browser if the script tag doesn't have it.

Also, Vite builds currently will return a console log if you are trying to use them without the `type="module"` parameter on a script tag if not they will throw exceptions from code, so it is mandatory to use `type="module"` with them. Parcel doesn't prevent this and their ESM compiled script will load fine but you should try to avoid it else it is counter productive, you should then use a CommonJS build.
