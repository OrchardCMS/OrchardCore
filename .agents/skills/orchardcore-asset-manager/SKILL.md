---
name: orchardcore-asset-manager
description: Builds, watches, and manages frontend assets in OrchardCore. Use when modifying SCSS, JS, TS, or Vue files, adding new assets to a module/theme, or troubleshooting build failures. Covers all asset actions (vite, sass, min, copy, parcel, webpack, concat) and the three-tier package structure.
---

# OrchardCore Asset Manager

Handles all frontend asset compilation for OrchardCore modules and themes. Assets are discovered via `Assets.json` files under `src/{OrchardCore.Modules,OrchardCore.Themes}/*/`.

## Node.js Version

The repo requires **Node.js 24.x** (pinned in `.node-version`). The build script auto-detects a mismatch and prompts:

```
1) Continue anyway
2) Abort
3) Install via fnm, Node.js 24.x and build
4) Install via Volta, Node.js 24.x and build
```

Always select **option 1** (continue) or **option 3** (install via fnm). The allowed Bash commands for this project pre-approve both:

```bash
echo "1" | yarn build          # continue with current Node.js
echo "3" | yarn build          # install Node 24 via fnm and build
```

## Build Commands

All commands run from the **repository root** (`/home/skrypt/repo/orchardcore`).

```bash
# Build all assets
echo "1" | yarn build

# Build a specific asset by name
echo "1" | yarn build -n media-app

# Build multiple assets
echo "1" | yarn build -n media-app,media-field

# Build by tag
echo "1" | yarn build -t admin

# Watch a specific asset (dev mode, auto-rebuilds on save)
echo "1" | yarn watch -n media-app

# Host with bundler dev server (HMR)
echo "1" | yarn host -n media-app

# Clean all build output
echo "1" | yarn clean
```

The `-n` name maps to the `"name"` field in `Assets.json`.

## Local Claude Code Setup (One-time, per developer)

`.claude/` is gitignored. To avoid being prompted on every build command, create `.claude/settings.json` locally with these pre-approved commands:

```json
{
  "permissions": {
    "allow": [
      "Bash(echo \"1\" | yarn build*)",
      "Bash(echo \"3\" | yarn build*)",
      "Bash(fnm exec --using 24.14.1 -- corepack yarn install*)",
      "Bash(yarn check*)",
      "Bash(yarn lint*)",
      "Bash(echo \"1\" | yarn dry-run*)"
    ]
  }
}
```

Do **not** commit this file or add a `.gitignore` exception for it — it is intentionally kept local to avoid silently pre-approving commands on other developers' machines.

## Prerequisites / Installing Dependencies

Before building, if packages are missing:

```bash
fnm exec --using 24.14.1 -- corepack yarn install
```

After modifying any `package.json` (root, `.scripts/assets-manager/`, or a module's `Assets/`), re-run install.

## Built Output

Built files go to the module/theme's `wwwroot/` folder and **must be committed** to the repo. After any asset change, commit both the source file and the generated `wwwroot/` files.

## Package Structure (Three-Tier)

| Location | Purpose |
|---|---|
| Root `package.json` | Workspace orchestration, version resolutions |
| `.scripts/assets-manager/package.json` | Build toolchain (Vite, Parcel, Sass, etc.) |
| `src/.../Assets/package.json` | Runtime deps bundled into that module/theme |

To add a **runtime dependency** to a module:
```bash
# From the module's Assets/ folder
yarn add some-library@1.2.3
# Then rebuild
echo "1" | yarn build -n asset-name
```

To add a **build tool** (affects all assets):
- Edit `.scripts/assets-manager/package.json`
- Run `fnm exec --using 24.14.1 -- corepack yarn install`

## Assets.json

Each module/theme with assets has an `Assets.json` at its root (e.g. `src/OrchardCore.Modules/OrchardCore.Media/Assets.json`). See `references/actions.md` for all supported actions.

Quick example:
```json
[
  {
    "action": "vite",
    "name": "media-app",
    "source": "Assets/media-app/",
    "tags": ["admin", "js"]
  },
  {
    "action": "sass",
    "name": "media-styles",
    "source": "Assets/scss/media.scss",
    "tags": ["admin", "css"]
  }
]
```

## Code Quality Commands

These do **not** go through the Node.js version prompt — run them directly.

```bash
# TypeScript type-check all Vue/TS files (vue-tsc --noEmit)
yarn check

# Lint all JS/TS/Vue files (ESLint)
yarn lint

# Preview which files would be built/copied without writing anything
echo "1" | yarn dry-run
echo "1" | yarn dry-run -n media-app   # scoped to one asset
```

- **`yarn check`** — runs `vue-tsc --noEmit`; catches type errors across all Vue/TS source files. Run before committing TS/Vue changes. Does **not** support `-n`; to check a specific module, point it at that module's tsconfig directly: `yarn vue-tsc --noEmit -p src/OrchardCore.Modules/OrchardCore.Media/Assets/media-app/tsconfig.json`
- **`yarn lint`** — runs ESLint across the repo. Accepts a file or directory to scope it: `yarn lint src/OrchardCore.Modules/OrchardCore.Media/Assets/`. Fix any errors before committing.
- **`yarn dry-run`** — logs what the build would do (copy targets, output paths) without touching any files. Useful when adding a new `Assets.json` entry to verify paths before the first real build.

## Troubleshooting

| Problem | Fix |
|---|---|
| `Cannot find package '@tailwindcss/vite'` | Run `fnm exec --using 24.14.1 -- corepack yarn install` |
| Parcel cache stale after deleting output | Run `echo "1" \| yarn clean` then rebuild |
| Changes not reflected | Confirm the built `wwwroot/` files changed; rebuild if not |

## References

- `references/actions.md` — All Assets.json action types with examples
</content>
</invoke>