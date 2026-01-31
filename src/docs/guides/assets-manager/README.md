# Assets Manager

Based on [Concurrently](https://github.com/open-cli-tools/concurrently) the Orchard Core assets management tool is used for building, watching, hosting assets. It allows to use bundlers as [Parcel](https://parceljs.org/) [Vite](https://vite.dev/) and [Webpack](https://webpack.js.org/). This is a non-opiniated tool which allows to be extended for any asset compiler/bundler that someone may require in the future. Everything is written as ES6 modules (.mjs files).

`Concurrently`, is a concurrent shell runner which allows to trigger any possible shell command.

Concurrently uses an `Assets.json` file that defines actions to execute.

Parcel is the easiest way to build assets so far as it doesn't require any configuration. It is a zero file configuration bundler which means we use the same configuration for all assets. It is the recommended builder for those who want to easily start with a bundler. Though, Vite is more suited for Vue apps.

## Prerequisites

1. Install the current 22.x version of [Node.js](https://nodejs.org/en/download). If you are already using a different version of Node.js for other projects, we recommend using Node Version Manager (see [here](https://github.com/nvm-sh/nvm) for the original project for *nix systems, and [here](https://github.com/coreybutler/nvm-windows) for Windows).
2. From the root of the repository, run the following commands. Be sure to indeed run **exactly** these, and verify that the Yarn version matches the `packageManager` value in the root `package.json` (currently v4.9.x).
    ```cmd
    REM On Windows may require to run command shell with administrator privileges.
    corepack enable 
    yarn
    ```

!!! danger
    Some third-party distributors may not include Corepack by default, in particular if you install Node.js from your system package manager. If that happens, running `npm install -g corepack` before `corepack enable` should do the trick.    

## Building assets if you change an SCSS, JS, or TS/TSX file

What to do if you change an SCSS, JS, or TS/TSX file in any of Orchard Core's projects, and want to update the output files (that go into the `wwwroot` folders)?

1. Make sure you completed the above "Prerequisites" steps. You should always run at least `yarn` to update the dependencies.
2. Run `yarn build` from the command line in the root of the repository. This will build all changed assets.

Alternatively, if you make a lot of changes during development that you want to test quickly, you don't need to run the full build every time. Instead, use `yarn watch` to automatically build assets when you save a file. For this, run `yarn watch -n asset-name`, where `asset-name` is the `name` property you can find for the given file in the `Assets.json` file of the given project's root folder. E.g., for the Audit Trail module's `audittrailui.scss` file it's `audittrail`, so the command is `yarn watch -n audittrail`. You can also watch multiple assets at once by separating their names with commas, e.g., `yarn watch -n audittrail, audit-trail-diff-viewer`.

## All features

- Build all assets: `yarn build`
- Build module by name: `yarn build -n asset-name`
- Build assets by tag: `yarn build -t tagname`
- Watch module by name: `yarn watch -n asset-name`.
- Host with bundler dev server: `yarn host -n asset-name`.
- Action on multiple assets with `-n` filter: `yarn {build, watch or host} -n asset-name1, asset-name2`  
- Clean folders with `yarn clean`. Will also clean parcel-cache folder.
- Makes use of the Yarn version pinned in the root `package.json`.
- Makes use of yarn workspaces which allows to import files from different locations in the app for sharing ES6 modules.
- VS Code launcher debug option added as "Asset Bundler Tool Debug"
- Assets.json file definitions for building assets.
- Concurrently will retry building up to 3 times making CI build less prone to fail.
- All tasks can be run from within Visual Studio with the [Task Runner Explorer extension](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.TaskRunnerExplorer).

## Random Notes

- The copy task does not watch for file changes and the watch commands do not copy files. The build command will copy files.
- Use dry-run to see a log of which files are being copied and which asset files we have in the solution.
- Parcel seems a bit picky with transpiling some libraries.

## Supported actions

### Parcel 

Runs the Parcel bundler.

**Note**: Sometimes, Parcel is too aggressive in its caching. If you manually delete any output folders, you may need to delete the `.parcel-cache` folder as well for Parcel to write to it again. Running the `yarn clean` command will clean it up for you. Also, you should set the "dest" folder of your Parcel assets to a different folder per asset defined in your Assets.json file. It is required because we are cleaning the folder before watching or building the files.

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

### Parcel Bundling

It is possible to bundle apps together when using Parcel by using the "bundleEntrypoint" param in the Assets.json file. This allows to bundle different apps together in your app even though they are not standing in the same directory. These files will be compiled in the folder set in your build.config.mjs file standing at the root of the solution. When using bundleEntrypoint parameter there is no need to set the `dest` param.

#### Examples

Parcel bundle output folder:

```javascript
export const parcelBundleOutput = "src/OrchardCore.Modules/OrchardCore.Resources/wwwroot/Scripts/bundle"
```

Parcel bundleEntrypoint parameter:

```json
[
  {
    "action": "parcel",
    "name": "module-microsoft-datasource-wrapper",
    "source": "Assets/Scripts/datasource-wrapper.js",
    "bundleEntrypoint": "bundle-name",
    "tags":["vue3"]
  }
]
```

#### Sourcemaps

For javascript files Parcel will create a .min.js file for you along with a .map file. The .min.js is created for use in production as it doesn't reference the .map file. Using the ResourceManagementOptionsConfiguration you will want to set it this way:

```C#
    _manifest
        .DefineScript("admin")
        .SetDependencies("bootstrap", "admin-main", "theme-manager", "jQuery", "Sortable")
        .SetUrl("~/TheAdmin/js/theadmin/TheAdmin.min.js", "~/TheAdmin/js/theadmin/TheAdmin.js")
        .SetVersion("1.0.0");
```

### Vite

Vite bundler action will support any configuration. From bundling a vue app to compiling a simple library. It is working by configuration file. The asset management tool simply loads a vite.config.ts file based on the source folder that we instruct it to use from the Assets.json file.

Example of Assets.json config file:

```json
[
  {
    "action": "vite",
    "name": "my-vue-app",
    "source": "Assets/vite-project/",
    "tags": ["admin", "dashboard", "js"]
  }
]
```

The source property must be the root folder of your Vite app where your vite.config.ts or .js file stands.

#### Getting Started with Vite

Create a new module or theme in Orchard Core. This module or theme needs to have a /Assets folder.
In this /Assets folder we will create a boilerplate Vue app using this command:

```cmd
cd src/OrchardCore.Modules/path-to-your-module/Assets
yarn create vite
```

Here is an example of a Vue app using Typescript:

```cmd
➤ YN0000: · Yarn 4.9.4
➤ YN0000: · Yarn 4.9.4
➤ YN0000: ┌ Resolution step
➤ YN0085: │ + create-vite@npm:6.2.0
➤ YN0000: └ Completed
➤ YN0000: ┌ Fetch step
➤ YN0013: │ A package was added to the project (+ 141.45 KiB).
➤ YN0000: └ Completed
➤ YN0000: ┌ Link step
➤ YN0000: └ Completed
➤ YN0000: · Done in 0s 258ms

√ Project name: ... vite-project
√ Select a framework: » Vue
√ Select a variant: » TypeScript

Scaffolding project in C:\repo\OrchardCore\src\OrchardCore.Modules\OrchardCore.Media\Assets\vite-project...

Done. Now run:

  cd vite-project
  yarn
  yarn dev
```

Now you could execute the commands that are suggested. It will start the Vite dev server with HMR feature. Though what we want is to execute the server by using the asset manager tool. We will need an Assets.json file for that matter.

Create an Assets.json file at the root of your new module. For example: "src/OrchardCore.Modules/PathToYourModule/Assets.json". This file should contain these settings:

```json
[
  {
    "action": "vite",
    "name": "my-vue-app",
    "source": "Assets/vite-project/",
    "tags": ["admin", "dashboard", "js"]
  }
]
```

This `Assets.json` file will instruct the asset manager tool to execute the Vite bundler and to look at the source folder for a `vite.config.ts` or `.js` file. But we need to define where we want these assets to be compiled. For that matter we will need to modify the `vite.config.ts` file.

Here is an example of a configuration file that the asset bundler will be able to work with in the context of a Vue app. Notice that we are using `path.resolve()` so that this configuration file always returns the appropriate relative path to the asset bundler. Also, it is required that you set an `outDir` so that the assets be compiled to that directory.

For more details, these configurations are well documented on Rollup.js and Vite.js websites.

```javascript
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path';
import { fileURLToPath } from "url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  build: {
    outDir: path.resolve(__dirname, '../../wwwroot/Scripts/Media2/'),
  },
})
```

Execute Vite dev server from the asset manager tool:

```sh
# Move to Orchard Core src folder
cd C:/repo/OrchardCore/src

# Install dependencies
yarn 

# start vite dev server
yarn host -n my-vue-app
```

Alternatively, execute Vite watcher:

```cmd
yarn watch -n my-vue-app
```

Or simply build that Vite app:

```cmd
yarn build -n my-vue-app
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

The config property must be the path to where your webpack.config.js file stands.

### Run

Allows to run any command through Concurrently.

```json
[
  {
    "action": "run",
    "name": "itwin-viewer-app",
    "source": "Assets/itwin-viewer-app",
    "scripts": {
      "build": "yarn build",
      "watch": "yarn start"
    }
  }
]
```

Here, the `source` property must be a folder and is used as the working directory where the command is ran.

The scripts keys must match the command used to start the pipeline. If you start the pipeline with `yarn build` this would run all `build` scripts of the run actions.

### Copy

Allows to copy files.

#### Single Source (String)
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

#### Multiple Sources (Array)

Consolidate multiple copy operations into a single entry:

```json
[
  {
    "action": "copy",
    "name": "bootstrap-4.6.1",
    "source": [
      "node_modules/bootstrap-4.6.1/dist/css/bootstrap.css",
      "node_modules/bootstrap-4.6.1/dist/css/bootstrap.min.css",
      "node_modules/bootstrap-4.6.1/dist/js/bootstrap.js",
      "node_modules/bootstrap-4.6.1/dist/js/bootstrap.min.js"
    ],
    "dest": "wwwroot/Vendor/bootstrap-4.6.1/",
    "tags": ["resources", "css", "js"]
  }
]
```

**Benefits:**

- ✅ Reduces duplication in Assets.json
- ✅ Groups related files together
- ✅ Easier to maintain and read
- ✅ Fully backward compatible with single string sources

**Notes:**

- The `source` field can be a string (single pattern) or an array of strings (multiple patterns)
- Each pattern in the array can be a file path, glob pattern, or wildcard
- The `dest` should always be a folder (we do not support renaming files)
- You can use the `dry-run` task to preview where files will be copied

#### Important Notes

##### Base Folder Detection

When using `**` in a source pattern, the base folder is auto-detected:

```json
{
  "source": "node_modules/bootstrap-4.6.1/dist/**"
}
```

- Base folder: `node_modules/bootstrap-4.6.1/dist/`
- Files are copied relative to this base folder
- Example: `dist/css/bootstrap.css` → `{dest}/css/bootstrap.css`

##### Mixing Patterns

You can mix different pattern types in the same array:

```json
{
  "source": [
    "node_modules/lib/dist/**",        // Recursive glob
    "node_modules/lib/extras/*.js",    // Single-level glob
    "node_modules/lib/readme.md"       // Specific file
  ]
}
```

##### Default Destination

If `dest` is not specified, the default destination is determined from tags and file extension of the **first source**:

```json
{
  "action": "copy",
  "name": "my-scripts",
  "source": [
    "node_modules/lib/file1.js",  // First source determines dest
    "node_modules/lib/file2.css"
  ],
  "tags": ["resources", "js"]
  // dest defaults to: "{basePath}/wwwroot/Scripts/"
}
```

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

The source field can be a file, or a glob.

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

The source field can be a file, or a glob.

The destination should always be a folder as we do not support renaming files.

You can use the dry-run task to log to the console where the files will be copied to.

### Concat

Allows to concatenate files together.

```json
[
  {
    "action": "concat",
    "name": "media",
    "source": [
      "node_modules/blueimp-file-upload/js/jquery.iframe-transport.js",
      "node_modules/blueimp-file-upload/js/jquery.fileupload.js",
      "Assets/js/app/Shared/uploadComponent.js"
    ],
    "dest": "wwwroot/Scripts"
  }
]
```

The source field must be an array of files.

The destination should always be a folder as we do not support renaming files.

#### Important: Concat Action and Node Modules

**The `concat` action physically concatenates files** - it does **not** use a bundler or module resolver. This has important implications for how it handles `node_modules` dependencies:

##### How Node Modules Resolution Works

When a source path starts with `node_modules/`, the concat action resolves it from the **workspace root** `node_modules/` directory (where Yarn hoists all workspace dependencies):

```javascript
// In assetGroups.mjs
if (src.startsWith("node_modules")) {
    // Always resolves to: <workspace-root>/node_modules/
    return path.resolve(path.join(process.cwd(), src)).replace(/\\/g, "/");
}
```

##### Version Constraint Requirements

**⚠️ Critical Limitation:** The `concat` action resolves dependencies from the **workspace root** `node_modules/` directory. This means all modules/themes using the same dependency with `concat` must coordinate their version requirements.

**Why?** Yarn workspaces use "selective hoisting":

- ✅ If all workspaces request the **same version**: Yarn hoists it to the root `node_modules/`
- ❌ If workspaces request **different versions**: Yarn hoists the most common version to root, and installs others locally in each workspace's `node_modules/`

Since `concat` **always** resolves from the root `node_modules/`, it will:

- ✅ Work correctly when all versions match (hoisted to root)
- ❌ **Fail** when different versions are needed (ignores local installs)

**Example Problem:**
```
OrchardCore.Media/Assets/package.json:     "bootstrap": "5.3.8"
OrchardCore.Resources/Assets/package.json:  "bootstrap": "5.3.8"  
SomeTheme/Assets/package.json:              "bootstrap": "4.6.1"  ← Different!
```

**Result:**

- `bootstrap@5.3.8` → hoisted to `node_modules/bootstrap/` (most common)
- `bootstrap@4.6.1` → installed in `SomeTheme/Assets/node_modules/bootstrap/`
- ❌ `concat` action will **always use 5.3.8** from root, even for SomeTheme

##### Solutions for Multiple Versions

**Recommended: Use NPM Package Aliasing**

The recommended approach to handle multiple versions is **NPM package aliasing**. This allows you to install different versions of the same package under unique names in the root `node_modules/`:

```json
// Assets/package.json (in your module)
{
  "dependencies": {
    "bootstrap": "5.3.8",                          // Current version
    "bootstrap-4.6.1": "npm:bootstrap@4.6.1"       // Legacy version (aliased)
  }
}
```

After running `yarn install`, both versions are hoisted to the root:
```
node_modules/
  bootstrap/             → 5.3.8
  bootstrap-4.6.1/       → 4.6.1 (aliased)
```

Now you can reference either version in your `Assets.json`:

```json
[
  {
    "action": "concat",
    "name": "modern-bootstrap",
    "source": [
      "node_modules/bootstrap/dist/js/bootstrap.js",           // Uses 5.3.8
      "Assets/js/modern-features.js"
    ],
    "dest": "wwwroot/Scripts"
  },
  {
    "action": "concat",
    "name": "legacy-bootstrap",
    "source": [
      "node_modules/bootstrap-4.6.1/dist/js/bootstrap.js",     // Uses 4.6.1
      "Assets/js/legacy-features.js"
    ],
    "dest": "wwwroot/Scripts"
  }
]
```

**Benefits:**

- ✅ Both versions are hoisted to root `node_modules/` and work with `concat`
- ✅ Managed via package managers (no manual file copying)
- ✅ Version updates are tracked in `package.json`
- ✅ No Git bloat from vendored files

See the [Managing Multiple Package Versions](#managing-multiple-package-versions) section and [NPM Aliasing Guide](./NPM_ALIASING_GUIDE.md) for complete documentation.

##### Best Practices

**1. Coordinate Versions Across Modules When Possible**
If all modules can use the same version, keep it simple:

```json
// All Assets/package.json files should have:
{
  "dependencies": {
    "bootstrap": "5.3.8",  // ← Same exact version everywhere
    "jquery": "3.7.1"       // ← Same exact version everywhere
  }
}
```

**2. Use Root Resolutions for Consistency**
Add a `resolutions` field in the root `package.json` to enforce version consistency when using a single version:

```json
// package.json (root)
{
  "resolutions": {
    "bootstrap": "5.3.8",
    "jquery": "3.7.1"
  }
}
```

**3. Use NPM Aliasing for Different Versions**
When different modules genuinely need different versions, use NPM package aliasing as shown above. This is the recommended approach for maintaining multiple versions while keeping them manageable through package managers.

**4. Alternative: Use a Bundler**
If you need complex dependency resolution or module bundling, consider using a bundler action (`parcel`, `webpack`, or `vite`) instead of `concat`. Bundlers properly resolve `node_modules` and handle version differences automatically.

**5. Last Resort: Local Assets Folder**
Only if NPM aliasing doesn't meet your needs, copy the required library files directly into your module's `Assets/` folder:

```json
[
  {
    "action": "concat",
    "name": "media",
    "source": [
      "Assets/vendor/blueimp-file-upload/jquery.iframe-transport.js",  // ← Local copy
      "Assets/vendor/blueimp-file-upload/jquery.fileupload.js",
      "Assets/js/app/Shared/uploadComponent.js"
    ],
    "dest": "wwwroot/Scripts"
  }
]
```

This approach trades convenience for complete version control, but eliminates the shared dependency issue. However, NPM aliasing is preferred as it keeps dependencies manageable and trackable.

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

Here is an example for Vite:

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

## ECMAScript vs CommonJS (Bundlers)

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

Also, now when adding a `<script>` tag to the HTML, you will need to use:

```html
<script type="module" src="somepath"></script>
```

`<script>` tags are non-ESM by default; you have to add a `type="module"` attribute to opt into ESM mode.

Meaning that the ESM script will be interpreted as CommonJS by the browser if the script tag doesn't have it.

Additionally, Vite by default only allows ES6 builds. It will log a message to the console if you attempt to use its .js builds without the `type="module"` attribute in the script tag; otherwise, it will throw exceptions in the code. Therefore, it is mandatory to include `type="module"` with Vite builds.

Parcel allows ES6 builds by setting the `"type": "module"` parameter in the package.json file. If this parameter is not set, Parcel will compile as CommonJS.

ESM compiled scripts will load fine in a script tag without the `type="module"` attribute. However, you should try to avoid this. 
For more details, see: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Modules#applying_the_module_to_your_html

## Managing Multiple Package Versions

We use **NPM package aliasing** to manage multiple versions of the same package without manual copying. See the [NPM Aliasing Guide](./NPM_ALIASING_GUIDE.md) for complete documentation.

**Quick Example:**
```json
// Assets/package.json
{
  "dependencies": {
    "vue": "3.5.13",                   // Latest version
    "vue-2.6.14": "npm:vue@2.6.14"     // Legacy version (aliased)
  }
}
```

This allows you to:

- ✅ Install multiple versions of the same package
- ✅ Reference them from `node_modules` in your `Assets.json`
- ✅ Update versions via NPM/Yarn instead of manual copying
- ✅ Avoid Git bloat from vendored packages

For detailed instructions, migration guides, and best practices, see [NPM_ALIASING_GUIDE.md](./NPM_ALIASING_GUIDE.md).

---

## Advanced: Architecture and Package Management

The following section describes the internal architecture of the asset management system. This is useful for advanced users who want to understand how the system works or need to manage dependencies.

### Package Structure Overview

The OrchardCore asset management system uses a **three-tier package structure** designed to separate concerns and minimize version conflicts:

#### 1. Root `package.json` - Workspace Orchestrator
**Location:** Root of the repository  
**Purpose:** 

- Manages Yarn workspaces for the monorepo
- Contains **general development dependencies** (ESLint, TypeScript, etc.)
- References the assets-manager CLI tool as a workspace dependency
- Defines npm scripts that invoke the assets-manager CLI
- Can provide version constraints via `resolutions` to prevent conflicts

**Why it's minimal:**

- Keeps the root focused on workspace orchestration
- Delegates build tooling to the assets-manager package
- Avoids dependency duplication across the workspace

#### 2. `.scripts/assets-manager/package.json` - Build Toolchain Package
**Location:** `.scripts/assets-manager/`  
**Purpose:**

- Contains **all build tool dependencies** (Parcel, Vite, Webpack, Sass, PostCSS, etc.)
- Provides the executable entry point (`bin: ./build.mjs`)
- Defines the assets-manager CLI tool metadata
- Centralizes all asset compilation tooling

**Why it contains build tools:**

- Single source of truth for all build tool versions
- Eliminates duplication across the monorepo
- Simplifies dependency updates (update once, applies everywhere)
- Yarn workspaces automatically hoists these to the root `node_modules`
- Clear separation: "What builds the assets" vs "What the workspace needs"

#### 3. Module/Theme `Assets/package.json` Files
**Location:** `src/OrchardCore.{Modules,Themes}/*/Assets/`  
**Purpose:**

- Contains **runtime dependencies** for the specific module/theme
- Libraries that get bundled into the application (jQuery, Vue, Bootstrap, etc.)
- These are the actual packages used by the frontend code

**Why they're separate:**

- Each module/theme has different runtime requirements
- Version constraints specific to that module's needs
- Clear separation: "What I ship" vs "What builds it"

### Dependency Flow

```
┌─────────────────────────────────────────────────────────┐
│ Root package.json                                   │
│ - Workspace orchestration                           │
│ - General dev dependencies (ESLint, TypeScript)     │
│ - References assets-manager as workspace dependency │
│ - Version constraints (resolutions)                 │
└────────────┬────────────────────────────────────────────┘
             │ (workspace dependency)
             ▼
┌─────────────────────────────────────────────────────────┐
│ .scripts/assets-manager/package.json                │
│ - Build tools (Parcel, Vite, Webpack, Sass)         │
│ - Asset compilation dependencies                    │
│ - Entry point: build.mjs                            │
└────────────┬────────────────────────────────────────────┘
             │ (processes)
             ▼
┌─────────────────────────────────────────────────────────┐
│ Module/Theme Assets/package.json                    │
│ - Runtime dependencies (jQuery, Vue, etc.)          │
│ - Libraries that get bundled                        │
│ - Module-specific versions                          |
└─────────────────────────────────────────────────────────┘
```

### Benefits of This Architecture

1. **Clear Separation:** Build tools vs workspace deps vs runtime dependencies are distinct
2. **No Duplication:** Build tools are defined once in assets-manager package
3. **No Version Conflicts:** `resolutions` field in root enforces consistency
4. **Easy Maintenance:** Update build tools in one place (assets-manager package)
5. **Workspace Efficiency:** Yarn hoisting optimizes disk usage and installation time

### How It Works

When you run `yarn build`:

1. The root `package.json` script calls `assets-manager build`
2. Yarn resolves `assets-manager` to `.scripts/assets-manager/build.mjs`
3. The build script uses build tools (Parcel/Vite/Webpack) from its own dependencies
4. For each module, it processes the `Assets.json` and bundles the runtime dependencies
5. Output goes to the module's `wwwroot` folder

### Managing Dependencies

#### Adding Build Tool Dependencies
To add or update a build tool (e.g., upgrading Parcel, adding a new PostCSS plugin):

1. Add/update it in the **`.scripts/assets-manager/package.json`** under `dependencies`
2. Run `yarn install` from the repository root
3. If you need to enforce a specific version across all workspaces, add it to the root `package.json` `resolutions`

**Example:**
```json
// .scripts/assets-manager/package.json
{
  "dependencies": {
    "parcel": "2.14.0"  // Update version
  }
}
```

```json
// package.json (root) - optional, for enforcing versions
{
  "resolutions": {
    "parcel": "2.14.0"  // Enforce everywhere
  }
}
```

#### Adding Module/Theme Runtime Dependencies
To add a library that your module needs at runtime (e.g., a Vue component library):

1. Navigate to your module's Assets folder: `cd src/OrchardCore.Modules/YourModule/Assets`
2. Add the dependency: `yarn add your-library`
3. The dependency is added to `Assets/package.json` in that module only
4. Import and use it in your module's asset files

**Example:**
```bash
cd src/OrchardCore.Modules/OrchardCore.Media/Assets
yarn add blueimp-file-upload@10.32.0
```

#### Preventing Version Conflicts
The root `package.json` can include a `resolutions` field to enforce specific versions:

```json
{
  "resolutions": {
    "postcss": "8.5.3",
    "sass": "^1.85.1",
    "glob": "^11.0.1"
  }
}
```

This ensures that even if different modules request different versions, Yarn will use the version specified in `resolutions`. This prevents version conflicts across the workspace.
