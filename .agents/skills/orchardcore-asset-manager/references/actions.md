# Assets.json Action Reference

All actions are entries in a module/theme's `Assets.json` file. The `name` field is the identifier used with `-n` in CLI commands. The `tags` array is used for `-t` filtering.

## vite

Runs a Vite bundler using the `vite.config.ts` in the source folder. Use for Vue apps and TypeScript projects.

```json
{
  "action": "vite",
  "name": "media-app",
  "source": "Assets/media-app/",
  "tags": ["admin", "js"]
}
```

- `source` — root folder of the Vite app containing `vite.config.ts`
- Output goes to wherever `build.outDir` is set in `vite.config.ts`
- The asset manager auto-applies the `orchard-minify` plugin: produces `.js`, `.min.js`, `.map` — do **not** set `build.minify` in `vite.config.ts`
- Do not add a workspace-level `package.json` in the Vite app folder; use the module's `Assets/package.json` instead

### Resource manifest example
```csharp
_manifest
    .DefineScript("media-app")
    .SetUrl("~/Media/Scripts/media2.min.js", "~/Media/Scripts/media2.js")
    .SetVersion("1.0.0");
```

## sass

Transpiles SCSS to CSS. Outputs to `wwwroot/Styles/` by default (or `dest` if specified).

```json
{
  "action": "sass",
  "name": "admin-dashboard",
  "source": "Assets/scss/dashboard.scss",
  "tags": ["admin", "css"]
}
```

- `source` — single `.scss` file or glob
- `dest` — optional output folder (defaults to `wwwroot/Styles/`)

## min

Minifies JS or CSS files. Produces `.min.js`/`.min.css` and `.map` files.

```json
{
  "action": "min",
  "name": "media-field",
  "source": "Assets/js/media-field.js",
  "dest": "wwwroot/Scripts/",
  "tags": ["admin", "js"]
}
```

- `source` — file or glob
- `dest` — optional output folder

## copy

Copies files (often from `node_modules`) to `wwwroot/`. Does not watch for changes.

```json
{
  "action": "copy",
  "name": "bootstrap-5",
  "source": [
    "node_modules/bootstrap/dist/css/bootstrap.min.css",
    "node_modules/bootstrap/dist/js/bootstrap.bundle.min.js"
  ],
  "dest": "wwwroot/Vendor/bootstrap-5/",
  "tags": ["resources"]
}
```

- `source` — string or array of strings/globs
- `dest` — output folder; if omitted, inferred from tags and first source extension
- `node_modules/` paths resolve from the **workspace root**

## concat

Concatenates multiple files into one. Does not use a bundler — physically joins the files.

```json
{
  "action": "concat",
  "name": "media",
  "source": [
    "node_modules/blueimp-file-upload/js/jquery.fileupload.js",
    "Assets/js/app/Shared/uploadComponent.js"
  ],
  "dest": "wwwroot/Scripts"
}
```

- `source` — array of files (no globs)
- All `node_modules/` paths resolve from workspace root — all modules must agree on the same version, or use **NPM aliasing** (e.g. `"bootstrap-4.6.1": "npm:bootstrap@4.6.1"`)

## parcel

Runs the Parcel bundler. Zero-config; reads `Assets/package.json` for the entry point.

```json
{
  "action": "parcel",
  "name": "datasource-wrapper",
  "source": "Assets/Scripts/datasource-wrapper.js",
  "dest": "wwwroot/datasource-wrapper",
  "tags": ["js"]
}
```

- `source` — entry point file
- `dest` — output **folder** (required; Parcel outputs multiple files)
- If cache goes stale after deleting output, run `yarn clean` first

## webpack

Runs Webpack using a config file.

```json
{
  "action": "webpack",
  "name": "graphiql",
  "config": "/Assets/webpack.config.js",
  "tags": ["admin", "js"]
}
```

- `config` — path to `webpack.config.js` (relative to the module root)

## run

Runs an arbitrary command via the asset manager. Useful for custom build scripts.

```json
{
  "action": "run",
  "name": "custom-app",
  "source": "Assets/custom-app",
  "scripts": {
    "build": "yarn build",
    "watch": "yarn start"
  }
}
```

- `source` — working directory for the command
- `scripts` — map of pipeline command (`build`/`watch`/`host`) to the shell command to run
</content>
</invoke>