# NPM Package Aliasing Guide

## Overview

OrchardCore's asset management system now uses **NPM package aliasing** to manage multiple versions of the same package without manual copying. This eliminates the need for manually vendored packages in the `Assets/Vendor/` folder, if an NPM package is available.

## What is NPM Package Aliasing?

NPM package aliasing allows you to install multiple versions of the same package under different names. For example:

```json
{
  "dependencies": {
    "vue": "3.5.13",              // Latest version
    "vue-2.6.14": "npm:vue@2.6.14" // Alias for older version
  }
}
```

This installs both versions into `node_modules/`:
- `node_modules/vue/` (version 3.5.13)
- `node_modules/vue-2.6.14/` (version 2.6.14)

## Benefits

✅ **No manual copying** - NPM handles all package installations  
✅ **Automated updates** - Run `yarn install` to sync all versions  
✅ **Version control** - All versions tracked in `package.json`  
✅ **No Git bloat** - Vendor files excluded from source control  
✅ **Consistent builds** - Same versions across all environments  

## How to Add a New Versioned Package

### 1. Add the alias to package.json

In your module's `Assets/package.json`:

```json
{
  "dependencies": {
    "bootstrap": "5.3.8",                    // Current version
    "bootstrap-4.6.1": "npm:bootstrap@4.6.1" // Legacy version
  }
}
```

### 2. Reference it in Assets.json

In your module's `Assets.json`:

```json
[
  {
    "action": "copy",
    "name": "bootstrap-4.6.1",
    "source": "node_modules/bootstrap-4.6.1/dist/**",
    "dest": "wwwroot/Vendor/bootstrap-4.6.1/",
    "tags": ["resources", "js"]
  }
]
```

### 3. Run yarn install

```bash
cd src
yarn install
```

### 4. Build the assets

```bash
yarn build
```

## Examples

### Multiple Vue Versions

```json
{
  "dependencies": {
    "vue": "3.5.13",
    "vue-2.6.14": "npm:vue@2.6.14"
  }
}
```

Assets.json:
```json
[
  {
    "action": "copy",
    "name": "vue2",
    "source": "node_modules/vue-2.6.14/dist/*.js",
    "dest": "wwwroot/Vendor/vue-2.6.14/"
  },
  {
    "action": "copy",
    "name": "vue3",
    "source": "node_modules/vue/dist/*.js",
    "dest": "wwwroot/Scripts/"
  }
]
```

### Multiple jQuery Versions

```json
{
  "dependencies": {
    "jquery": "3.7.1",
    "jquery-3.6.0": "npm:jquery@3.6.0",
    "jquery-3.5.1": "npm:jquery@3.5.1"
  }
}
```

### Multiple Font Awesome Versions

```json
{
  "dependencies": {
    "@fortawesome/fontawesome-free": "7.1.0",
    "@fortawesome/fontawesome-free-6.7.2": "npm:@fortawesome/fontawesome-free@6.7.2",
    "@fortawesome/fontawesome-free-6.6.0": "npm:@fortawesome/fontawesome-free@6.6.0"
  }
}
```

## Migration from Manual Vendor Files

If you have existing manually vendored packages:

### Before (Manual Vendor)

```
src/OrchardCore.Modules/YourModule/
├── Assets/
│   └── Vendor/
│       ├── vue-2.6.14/     ← Manually copied
│       └── bootstrap-4.6.1/ ← Manually copied
└── Assets.json
```

Assets.json:
```json
{
  "action": "copy",
  "name": "vue2",
  "source": "Assets/Vendor/vue-2.6.14/dist/*.js",
  "dest": "wwwroot/Vendor/vue-2.6.14/"
}
```

### After (NPM Aliasing)

```
src/OrchardCore.Modules/YourModule/
├── Assets/
│   └── package.json  ← Defines aliases
└── Assets.json       ← References node_modules
```

package.json:
```json
{
  "dependencies": {
    "vue-2.6.14": "npm:vue@2.6.14"
  }
}
```

Assets.json:
```json
{
  "action": "copy",
  "name": "vue2",
  "source": "node_modules/vue-2.6.14/dist/*.js",
  "dest": "wwwroot/Vendor/vue-2.6.14/"
}
```

### Migration Steps

1. **Add aliases to package.json**
   ```bash
   cd src/OrchardCore.Modules/YourModule/Assets
   # Edit package.json to add aliases
   ```

2. **Update Assets.json paths**
   - Change `Assets/Vendor/package-name/` to `node_modules/package-alias/`

3. **Install dependencies**
   ```bash
   cd src
   yarn install
   ```

4. **Remove manual vendor files**
   ```bash
   # After verifying build works:
   rm -rf src/OrchardCore.Modules/YourModule/Assets/Vendor/
   ```

5. **Update .gitignore** (if needed)
   ```
   # Make sure vendor files aren't tracked
   **/Assets/Vendor/
   ```

## Troubleshooting

### Package Not Found

**Error:** `ENOENT: no such file or directory, stat 'node_modules/package-name'`

**Solution:** Run `yarn install` from the repository root:
```bash
cd src
yarn install
```

### Wrong Version Copied

**Problem:** The build copies the wrong version of a package.

**Solution:** Check your alias name matches the path in Assets.json:
```json
// package.json
"vue-2.6.14": "npm:vue@2.6.14"

// Assets.json - alias name must match folder name
"source": "node_modules/vue-2.6.14/dist/*.js"
```

### Workspace Dependency Conflicts

**Problem:** Different modules need different versions of the same package.

**Solution:** This is exactly what aliasing solves! Add the alias to each module's `Assets/package.json`:

```json
// Module A needs Bootstrap 4
{
  "dependencies": {
    "bootstrap-4.6.1": "npm:bootstrap@4.6.1"
  }
}

// Module B needs Bootstrap 5
{
  "dependencies": {
    "bootstrap": "5.3.8"
  }
}
```

Both versions will be installed and available.

## Best Practices

### 1. Use Semantic Alias Names

Include the version number in the alias name for clarity:
```json
"vue-2.6.14": "npm:vue@2.6.14"  // ✅ Good
"vue2": "npm:vue@2.6.14"        // ⚠️ Less clear
```

### 2. Document Required Versions

Add comments in package.json to explain why legacy versions are needed:
```json
{
  "dependencies": {
    "vue": "3.5.13",
    // Required for legacy admin components - DO NOT REMOVE
    "vue-2.6.14": "npm:vue@2.6.14"
  }
}
```

### 3. Keep Aliases in Resources Module

For shared libraries used across multiple modules, define the alias in `OrchardCore.Resources/Assets/package.json` so all modules can reference the same version.

### 4. Clean Builds

When changing versions, do a clean build:
```bash
yarn clean
yarn install
yarn build
```

## Advanced: Version Constraints

You can use version ranges in aliases:
```json
{
  "dependencies": {
    // Exact version
    "bootstrap-4.6.1": "npm:bootstrap@4.6.1",
    
    // Latest in 4.x series
    "bootstrap-4-latest": "npm:bootstrap@^4.6.0",
    
    // Any 4.x version
    "bootstrap-4": "npm:bootstrap@~4.0.0"
  }
}
```

**Recommendation:** Use exact versions for predictable builds.

## See Also

- [NPM Documentation on Package Aliasing](https://docs.npmjs.com/cli/v9/configuring-npm/package-json#dependencies)
- [Assets Manager Documentation](./README.md)
- [Yarn Workspaces](https://yarnpkg.com/features/workspaces)
