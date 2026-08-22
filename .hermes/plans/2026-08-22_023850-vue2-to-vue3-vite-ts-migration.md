# Vue 2 to Vue 3 (Vite + TypeScript) Migration Plan

> For Hermes: use the subagent-driven-development skill to implement this plan task by task, one PR-sized chunk at a time. Do not implement in this turn - this is a plan document only.

**Goal:** Eliminate every remaining Vue 2 usage in the OrchardCore first-party codebase, replacing it with Vue 3 (Composition API + `<script setup lang="ts">`) built through Vite, on a new branch based on `skrypt/yarn-check`.

**Architecture:** Two very different kinds of Vue 2 usage exist today and need two different treatments:
1. One genuine npm-installed Vue 2 SFC app (`OrchardCore.AdminMenu`'s icon/permission pickers is NOT this - see inventory) actually there is exactly one npm package pinned to Vue 2: `src/OrchardCore.Modules/OrchardCore.AdminMenu/Assets/package.json` (`"vue": "2.7.16"`), but grep shows no `.vue` SFC files under that module - it is unused/stale and should just be deleted.
2. The real work is ~20 hand-written **global-script, Options-API, UMD-style** `new Vue({...})`/`Vue.component(...)` instances loaded via `<script asp-name="vuejs:2">` + inline `<script id="...">` X-Template strings in `.cshtml` views. These share one global `vuejs` resource (`~/OrchardCore.Resources/Vendor/vue-2.7.16/vue.js`) and a global `vue-multiselect:2`/`vue-draggable:2` pair. They need converting to small Vite-built Vue 3 SFC (or `<script setup>` TS) mini-apps, each with its own `Assets.json` `"action": "vite"` entry, following the exact pattern the repo has already proven twice: `OrchardCore.Media/Assets/media-picker` and `OrchardCore.Media/Assets/media-gallery`.

**Tech Stack:** Vue 3.5.x (already the pinned version everywhere else in the repo), Vite (already used by media-picker/media-gallery, and already a dependency of `.scripts/assets-manager`), TypeScript, `<script setup>` SFCs, `vue-tsc` for type-checking (already wired via `yarn check`), `vue-multiselect@3.2.0` (already present, already the Vue-3-compatible version, already loaded via `vuejs:3` dependency chain) to replace `vue-multiselect@2.1.6`.

---

## Current State Inventory (verified against `skrypt/yarn-check` @ commit `d0fa010358`)

### Vue 3 already in place (do not touch)
- `.scripts/assets-manager` tooling itself: `vue@^3.5.40`, `@vitejs/plugin-vue@^6.0.8` - the build pipeline is already Vue-3-ready.
- `OrchardCore.Resources/Assets/package.json`: `vue@^3.5.40`, ships the global `vuejs` resource at version 3.5.13 (`vue.global.js`) alongside the legacy 2.7.16 build.
- `OrchardCore.Media/Assets/media-picker` (9 `.vue` SFCs) and `OrchardCore.Media/Assets/media-gallery` (16 `.vue` SFCs, incl. `App.vue`) - both full Vite + Vue 3 + TypeScript apps already. Use these as the reference pattern for every new mini-app below.
- `vue-multiselect@3.2.0` is already vendored and registered as `vue-multiselect` + `vuejs:3` dependency in `ResourceManagementOptionsConfiguration.cs` (lines ~1177-1192) - it is NOT currently used by any view, but it is ready to be wired in.

### Vue 2 surface to migrate (the actual scope of this plan)

**A. Stale/unused npm package - delete, not migrate**
- `src/OrchardCore.Modules/OrchardCore.AdminMenu/Assets/package.json` declares `"vue": "2.7.16"` but the module has zero `.vue` files and its two Vue instances (icon-picker, permission-picker) are hand-written global-script JS, not SFCs. This dependency appears to be vestigial. Task 0 removes it (after confirming nothing imports it).

**B. Global-script `new Vue({...})` / `Vue.component(...)` instances (the real scope) - grouped by shared pattern:**

| # | Module | File(s) | Vue-multiselect? | vue-draggable? | Notes |
|---|---|---|---|---|---|
| 1 | OrchardCore.Cors | `Assets/Admin/cors-admin.js` | no | no | `Vue.component` global registration (`options-list`, `policy-details`) + root `window.corsApp`. X-Templates in `Views/Admin/Index.cshtml`. |
| 2 | OrchardCore.ContentFields | `Assets/js/OptionsEditor/optionsEditor.js` | no | yes | Feeds `TextFieldPredefinedListEditorSettings.Edit.cshtml`, `MultiTextFieldSettings.Edit.cshtml`. |
| 3 | OrchardCore.ContentFields | `Assets/js/vue-multiselect-userpicker.js` | yes | yes (+ Sortable) | `UserPickerField.Edit.cshtml`. |
| 4 | OrchardCore.ContentFields | `Assets/js/vue-multiselect-multitextfieldpicker.js` | yes | no | `MultiTextField-Picker.Edit.cshtml`. |
| 5 | OrchardCore.OpenId | `Assets/js/parametersEditor/parametersEditor.js` | no | yes | `OpenIdClientSettings.Edit.cshtml`. |
| 6 | OrchardCore.Forms | `Assets/js/SelectOptionsEditor/selectOptionsEditor.js` | no | yes | `SelectPart.Fields.Edit.cshtml`. AJAX-widget-reachable (Forms module, see PR #19522/#19489 notes on AJAX injection risk) - needs extra care re: init-on-insert, not just init-on-load. |
| 7 | OrchardCore.Forms | `Assets/js/form-visibility.js` | no | no | `FormInputElementVisibility.Edit.cshtml`; already has a co-located ES-module wrapper `form-input-element-visibility.ts` calling into it - migration should fold the Vue app directly into that TS module. |
| 8 | OrchardCore.Seo | `Assets/js/customMetaTagsEditor.js` | no | yes | `SeoMetaPart.Edit.cshtml`. Also AJAX-widget-reachable (per the PR #19489 description's own coverage note). |
| 9 | OrchardCore.Localization | `Assets/js/optionsEditor.js` | no | no | `LocalizationSettings.Edit.cshtml`. |
| 10 | OrchardCore.Menu | `Assets/js/menu-permission-picker.js` | yes | no | `MenuItemPermissionPart.Edit.cshtml`; already has a co-located ES-module wrapper `menu-item-permission-part.ts`. |
| 11 | OrchardCore.AdminMenu | `Assets/js/admin-menu-permission-picker.js` | yes | no | `LinkAdminNode.Fields.TreeEdit.cshtml`, `PlaceholderAdminNode.Fields.TreeEdit.cshtml`. Near-duplicate of #10 - consider a genuinely shared component this time instead of two near-identical files. |
| 12 | OrchardCore.AdminMenu | `Assets/js/admin-menu-icon-picker.js` | no | no | Same two views as #11. Wraps `fontawesome-iconpicker` (stays jQuery per the PR #19489 "intentionally still jQuery" list) - only the Vue shell around it migrates, exactly like the Trumbowyg precedent already set. |
| 13 | OrchardCore.Taxonomies | `Assets/js/tags-editor.js` | yes | no | `TaxonomyField-Tags.Edit.cshtml`. |
| 14 | OrchardCore.Shortcodes | `Assets/js/shortcodes.js` | no | no | `ShortcodeModal.cshtml`. |
| 15 | OrchardCore.Shortcodes | `Assets/js/shortcode-templates.js` | yes | no | `Admin/Edit.cshtml`, `Admin/Create.cshtml`. |
| 16 | OrchardCore.Resources | `Assets/js/vue-multiselect-wrapper.js` | yes | yes | Generic reusable wrapper, several consumers via `depends-on="vue-multiselect-wrapper"` - check all call sites before migrating (search `initVueMultiselect(`). |
| 17 | OrchardCore.Flows | `Assets/ts/content-type-picker.ts` | no | no | Already TypeScript, but instantiates the global Vue 2 UMD via a hand-typed `declare const Vue` shim (see file header comment, lines 5-8) - this is arguably the easiest one: swap the shim + template string for a real Vue 3 `createApp` call, same file, same build action (`parcel`, not `vite` - it's a single small file, no need to move it to a Vite mini-app). |

**C. Global resource plumbing to retire once (B) is fully migrated - `ResourceManagementOptionsConfiguration.cs`:**
- `vuejs:2` DefineScript (lines ~1127-1141) and its Vendor files (`OrchardCore.Resources/Vendor/vue-2.7.16/`).
- `vue-multiselect:2` DefineScript (lines ~1159-1169) and its Vendor files (`vue-multiselect-2.1.6/`).
- `vue-draggable` DefineScript (lines ~1237-1250ish) and its Vendor files (`vue-draggable-2.24.3/`) - only if nothing outside this list still needs Vue-2-flavoured draggable (grep confirms only the 5 "vue-draggable? yes" rows above use it).
- Matching `Assets.json` vendor-copy entries in `OrchardCore.Resources/Assets.json` (search `vue-2.7.16`, `vue-multiselect-2.1.6`, `vue-draggable-2.24.3`).
- Leave `vuejs:3` / `vue-multiselect:3.2.0` exactly as-is - they become the only versions.

---

## Proposed Approach

Do **not** attempt a single flag-day PR converting all 17 call sites at once - that mirrors the "45-commit divergence, 33 conflicting files" pain already hit once this session on the ES-module branch, at ~3x the file count. Instead:

1. **One branch, many small commits/PRs**, each migrating 1-3 closely related call sites (grouped by shared component pattern above, e.g. all 4 `vue-multiselect`+permission-picker sites as one PR, all 3 `vue-draggable`+options-editor sites as another). Each commit must leave `main`/the branch buildable and functionally green - never a half-migrated global resource.
2. **Global resource retirement is the LAST step**, not the first - `vuejs:2`/`vue-multiselect:2`/`vue-draggable` stay defined (harmlessly unused) until every consumer in section B is migrated, then one final cleanup commit removes them plus their vendor files. This avoids ever having a broken intermediate state where some view still expects the Vue 2 global that got deleted.
3. **Each migrated call site becomes its own small Vite-built Vue 3 mini-app**, following the `media-picker`/`media-gallery` precedent exactly: own `package.json`, own `src/` with `.vue` SFCs + a thin `main.ts` mount entry, own `Assets.json` `"action": "vite"` entry, own `vue-tsc --noEmit` wired into `yarn check` (already global via the root config - verify each new `package.json` picks it up the same way media-picker's does).
4. **Preserve every existing X-Template string's rendered HTML/CSS output exactly** - these are Bootstrap-classed admin UI, not visual redesigns. Convert `<script type="text/x-template" id="...">` blocks into the new SFC's `<template>` block essentially verbatim, translating only what Vue 3 syntax requires (see Compatibility Notes below), not restyling.
5. **AJAX-reachable views (#6 Forms/SelectPart, #8 Seo/SeoMetaPart) need explicit re-init-on-insert testing** - these are exactly the class of view already proven to have shipped a real bug once (PR #19522/#19489's Features-page regression, and the whole reason `observeAndInit.ts` exists). Each Vue 3 mount call must be wrapped the same way the ES-module editors already are (see `.scripts/bloom/helpers/observeAndInit.ts` for the existing pattern) rather than a bare `DOMContentLoaded` listener.
6. **Functional test coverage before/after** - for any of the 17 call sites that currently has Playwright coverage (check `test/OrchardCore.Tests.Functional/Tests/Cms/*.cs` per-module before starting; e.g. Seo/SeoMetaPart is called out as untested in tracking issue #19772), either add a minimal smoke test in the same PR or explicitly note the gap - do not silently rely on manual QA for a full framework-API rewrite the way the initial ES-module PR did for the Features page.

## Vue 2 → Vue 3 Compatibility Notes (apply consistently across every migrated file)

- **Global API → `createApp`**: `new Vue({...})` becomes `createApp({...}).mount(selector)`; `Vue.component('x', {...})` (global component registration) becomes either a local `components: {}` entry passed to `createApp`, or (preferred, matches media-picker/media-gallery's actual pattern) a separate imported `.vue` SFC.
- **`el:` option removed**: Vue 3's `createApp(...).mount(el)` takes the mount target as a `.mount()` argument, not a constructor option.
- **`data()` must always be a function** (already true in every file inventoried above - good, no change needed there).
- **`this.$emit` unchanged**, but prefer explicit `defineEmits` in `<script setup>` SFCs going forward.
- **Filters removed** - grep each file for `{{ x | y }}` template syntax; none were spotted in this pass's snippets but re-check the full X-Template bodies (not just the `.js` files) during each migration, since Options-API templates live separately in the `.cshtml` views.
- **`v-model` on custom components**: default prop/event renamed from `value`/`input` to `modelValue`/`update:modelValue` - anywhere a migrated mini-app's SFC is used with `v-model` internally, update accordingly (external `v-model` usage on native inputs is unaffected).
- **Vue 2 UMD global (`vuejs:2` resource, `Vue.component`, `new Vue(...)`) → real Vue 3 npm import** everywhere - no more `declare const Vue` global shims (retire the pattern from `content-type-picker.ts` too).
- **vue-multiselect 2.1.6 → 3.2.0**: API is close but not identical - re-check each of the 6 sites that use it (rows 3, 5→no wait check table, 10, 11, 13, 15, 16) against `vue-multiselect@3.2.0`'s docs/CHANGELOG for prop/event renames before assuming a drop-in swap. This library is already vendored and registered (`vuejs:3` dependency chain) - it is the "already done" half of this migration, just unused today.
- **vue-draggable 2.24.3 → `vuedraggable@next` (Vue 3 build) or drop entirely in favour of the repo's own `sortable-menu.ts`/SortableJS pattern**: given this PR's sibling work already replaced jQuery UI's `nestedSortable` with a hand-rolled SortableJS component (`sortable-menu.ts`) rather than reaching for a Vue wrapper library, seriously consider doing the same here instead of pulling in `vuedraggable@next` - it would remove a dependency rather than just upgrading it. Flag this as an open decision for task 1 rather than assuming either direction.

## Step-by-Step Plan

### Task 0: Create the branch and remove the stale AdminMenu Vue 2 package.json entry

**Objective:** Establish the working branch and clear out dead weight before real migration work starts.

**Files:**
- Modify: `src/OrchardCore.Modules/OrchardCore.AdminMenu/Assets/package.json`

**Steps:**
1. `git fetch origin skrypt/yarn-check --quiet && git checkout -B skrypt/vue3-migration origin/skrypt/yarn-check`
2. Confirm no `.vue` files exist under `OrchardCore.AdminMenu`: `search_files(pattern="*.vue", target="files", path="src/OrchardCore.Modules/OrchardCore.AdminMenu")` - expect zero results.
3. Confirm nothing imports the `vue` package from that `package.json` specifically (it's workspace-scoped per Yarn's PnP/workspaces setup) - check `yarn why vue` inside that workspace, or simply remove the dependency line and run `yarn install --immutable` + `yarn build -n admin-menu-node-list` (its only real build target) to confirm nothing breaks.
4. Remove the `"vue": "2.7.16"` line from `Assets/package.json`.
5. `yarn install --immutable && dotnet build OrchardCore.slnx -c Release -p:TreatWarningsAsErrors=true --warnaserror -p:RunAnalyzers=true -p:NuGetAudit=false` - expect clean.
6. Commit: `git add -A && git commit -m "Remove stale unused Vue 2 dependency from AdminMenu package.json"`.

### Task 1: Decide and document the vue-draggable replacement strategy

**Objective:** Resolve the open compatibility-notes question before it's independently re-decided 5 different ways across 5 different PRs.

**Status: DECIDED.** See "## Vue-draggable replacement decision" below.

**Steps:**
1. Read `.scripts/bloom/components/sortable-menu.ts` in full to understand the existing hand-rolled SortableJS pattern this repo already committed to for Menu/Taxonomies.
2. Evaluate `vuedraggable@next` (Vue 3 build) vs. reusing/extending SortableJS directly for the 5 vue-draggable consumers (rows 2, 3, 5, 6, 8 in the table above) - these are all simple flat reorderable lists (option rows, parameter rows, meta-tag rows), not nested trees, so a much smaller SortableJS wrapper than `sortable-menu.ts` would suffice if going that route.
3. Write the decision (with rationale) as a short `## Vue-draggable replacement decision` addendum to this plan file before starting Task 3 (the first list-editor migration) - do not silently decide per-file.
4. No code changes in this task - it's a decision checkpoint only.

## Vue-draggable replacement decision

**Decision (confirmed with user 2026-08-22): use `vuedraggable@next` (the Vue 3 build of `vuedraggable`), not a hand-rolled SortableJS component.**

Rationale: `sortable-menu.ts` (read in full per step 1) solves a genuinely harder problem - a single flat depth-tagged list standing in for a nested tree, with live indent/outdent preview and detached-subtree reinsertion, none of which any of the 5 `vue-draggable` consumers need. Their shape is uniformly "reorder N flat rows of a simple object array" (options, parameters, meta-tags), which is exactly `vuedraggable`'s own primary use case, not one that benefits from bespoke logic. `vuedraggable@next` wraps SortableJS under the hood, so this doesn't reintroduce jQuery UI or move away from the SortableJS foundation the rest of the app already standardized on - it's the same underlying library, just via the maintained Vue-binding rather than a second hand-written wrapper alongside `sortable-menu.ts`. Trade-off accepted: one additional npm dependency (`vuedraggable@next`), in exchange for less new code to write and maintain across Task 3's shared `options-table-editor.vue` and Task 5's `vue-multiselect-userpicker` (which also uses `vue-draggable` alongside `vue-multiselect`).

Practical implications for Task 3/Task 4/Task 5:
- Add `vuedraggable` (the `next` dist-tag / Vue-3-compatible major version) to whichever `package.json` ends up hosting `options-table-editor.vue` (see Task 3's build-location decision) and to `ContentFields`' `vue-multiselect-userpicker` migration target.
- Replace the old global `<draggable v-model="data.options" :tag="'tbody'">` (Vue 2 UMD registration via `vue-draggable:2`) with a real `import draggable from "vuedraggable"` + local component registration; `v-model` binding stays conceptually the same (row array in, reordered array out) but confirm the exact prop name (`v-model` vs `modelValue`/`list`) against the installed package's actual API before assuming parity, same caution as vue-multiselect's own 2→3 jump.
- The old global `vue-draggable:2` / `vue-draggable-2.24.3` `ResourceManagementOptionsConfiguration.cs` DefineScript and vendor files are retired in Task 7 once no consumer references them anymore (unchanged from the original plan).

### Task 2: Migrate the Flows content-type-picker (single-file, no extraction needed)

**Objective:** Lowest-risk migration first - it's already TypeScript, already `parcel`-built as a single file, and has no vue-multiselect/vue-draggable dependency.

**Files:**
- Modify: `src/OrchardCore.Modules/OrchardCore.Flows/Assets/ts/content-type-picker.ts`
- Modify: `src/OrchardCore.Modules/OrchardCore.Flows/ResourceManifest.cs` (the `content-type-picker` DefineScript's `depends-on`, if it currently lists `vuejs`)
- Modify: `src/OrchardCore.Modules/OrchardCore.Flows/Views/BagPart-Blocks.Edit.cshtml`, `FlowPart-Blocks.Edit.cshtml` (remove `<script asp-name="vuejs">`/equivalent if present, since it becomes a bundled import instead of a global dependency)
- Test: extend or add to `test/OrchardCore.Tests.Functional/Tests/Cms/FlowPartWidgetLifecycleTests.cs` (existing suite already opens the content-type-picker modal as part of "add widget" - verify it still exercises this path post-migration, no new test file necessarily needed)

**Steps:**
1. Read the full 277-line `content-type-picker.ts` file (already partially read - lines 1-40 seen; read 41-277 too).
2. Replace the `declare const Vue: new (...) => VueInstance` global shim + `import evalScripts` pattern with `import { createApp } from "vue"` at the top.
3. Convert the X-Template (find it in `ContentTypePicker.cshtml`, already partially read) into the SFC-equivalent `template:` option string (staying Options API here is fine and lower-risk than also switching to `<script setup>` in the same change - this file mixes vanilla-DOM modal logic with the Vue instance, a full-SFC extraction is a bigger, separate follow-up if wanted later).
4. Swap `new Vue({...})` for `createApp({...}).mount(...)`; capture the returned app instance (Vue 3's `createApp` return, not the Vue 2 constructor instance) for whatever `contentTypePickerApp` currently does with it downstream (check `flow-part-blocks.ts`/`bag-part-blocks.ts` for how it's consumed).
5. Update `ResourceManifest.cs`/`Assets.json`/view `depends-on` attributes to point at `vuejs` (the Vue 3 default, no `:2` qualifier) if the dependency chain needs it explicitly, or drop it if Vue is now a bundled npm import with no runtime global dependency at all (preferred - check whether `content-type-picker.ts`'s `parcel` build already bundles its imports, which it should).
6. Run: `yarn build -n content-type-picker` (verify the asset-manager group name via `Assets.json`'s `"name"` field, adjust if different).
7. Manually smoke-test via `orchardcore-tester` skill: open a Flow/Bag part editor, click "Add Widget", verify the content-type picker modal renders identically (search box, category pills, content-type cards) and selecting a type still adds the widget.
8. Run: `dotnet test test/OrchardCore.Tests.Functional/OrchardCore.Tests.Functional.csproj --filter-class "*FlowPartWidgetLifecycleTests*" --filter-class "*EsModuleEditorTests*"` - expect green (these exercise the "Add Widget" flow this modal is part of).
9. Commit: `git commit -m "Migrate Flows content-type-picker from Vue 2 UMD global to Vue 3"`.

### Task 3: Build the shared OptionsTableEditor bloom component, then migrate its 4 consumers

**Objective:** Four call sites (ContentFields x2 views, OpenId, Seo) are near-identical copies of the same "draggable key/value(s) table + JSON-edit-modal" widget - unify them into ONE shared Vue 3 component in `.scripts/bloom/components/` instead of porting each copy independently. (Localization's `optionsEditor.js` is a structurally different culture-picker widget wearing the same file name - it does NOT belong in this unification; migrate it standalone in Task 5.)

**Design:**
- New file: `.scripts/bloom/components/options-table-editor.vue` - a generic Vue 3 `<script setup lang="ts">` SFC taking:
  - `modelValue: Record<string, string>[]` (v-model) - the row array
  - `columns: { key: string; label: string; placeholder?: string }[]` - ordered text-input columns (2 for ContentFields/OpenId's `name`/`value`, 5 for Seo's `content`/`name`/`property`/`httpEquiv`/`charset`)
  - `defaultColumn?: { key: string; label: string }` - optional "mark as default" checkbox column (ContentFields only; OpenId/Seo omit it)
  - `addLabel: string` - the "Add a ..." button text (varies per consumer: "option"/"parameter"/"custom meta tag")
  - a companion modal sub-component (or a `showJsonModal` slot/prop) for the raw-JSON-textarea fallback editor, reusing the same generic row shape
- New file: `.scripts/bloom/helpers/options-table-editor-mount.ts` (or similar) - a thin typed helper mirroring the existing `initMonacoJsonSettingsEditor`-style pattern (element + data-attributes in, `createApp(...).mount(...)` out), so each consuming module's `.ts` entry stays a 2-3 line wrapper reading its view's `data-*` attributes and column config, not a copy of the component itself.
- **Decision needed before starting:** ContentFields' own two current views disagree on "default" selection UI - `TextFieldPredefinedListEditorSettings.Edit.cshtml` uses a radio bound to a shared form field name, `MultiTextFieldSettings.Edit.cshtml` uses a per-row checkbox (`option.default`). Standardizing on checkbox (simpler, no cross-row shared-name coupling to replicate) is the default recommendation - confirm with the user before implementing rather than silently picking one, since it's a small user-visible behavior change (single-select radio vs multi-select checkbox) for the predefined-list-with-radio case specifically.

**Files:**
- Create: `.scripts/bloom/components/options-table-editor.vue`
- Create: `.scripts/bloom/helpers/options-table-editor-mount.ts` (naming TBD to match existing bloom helper conventions - check `.scripts/bloom/helpers/` for the actual pattern in use before naming)
- Modify (each becomes a thin wrapper calling the shared component): `src/OrchardCore.Modules/OrchardCore.ContentFields/Assets/js/OptionsEditor/optionsEditor.js` → replace entirely, wire from `options-editor-fields.ts` (already exists, already ES-module, already the consumer-side entry point per the view's `depends-on="optionsEditor"` chain)
- Modify: `src/OrchardCore.Modules/OrchardCore.OpenId/Assets/js/parametersEditor/parametersEditor.js` → replace, wire from `openid-client-settings.ts` (already exists)
- Modify: `src/OrchardCore.Modules/OrchardCore.Seo/Assets/js/customMetaTagsEditor.js` → replace, wire from `seo-meta-part.ts` (already exists)
- Modify views (extract X-Templates, since the template now lives in the SFC, not inline `<script type="text/x-template">`): `TextFieldPredefinedListEditorSettings.Edit.cshtml`, `MultiTextFieldSettings.Edit.cshtml`, `OpenIdClientSettings.Edit.cshtml`, `SeoMetaPart.Edit.cshtml` - each keeps only its mount-point `<div data-*="...">` and drops the `<script type="text/x-template">` blocks entirely.
- Test: `SeoMetaPart.Edit.cshtml`'s custom-meta-tags editor is confirmed untested per tracking issue #19772 and is AJAX-widget-reachable (SeoMetaPart can live on a Bag/Flow widget) - add a Playwright test exercising the AJAX-injected case specifically, not just full-page-load, per the Approach section's AJAX-reachability risk note.

**Steps:**
1. Get the radio-vs-checkbox decision confirmed with the user (see Design note above) before writing the component.
2. Read `.scripts/bloom/components/` and `.scripts/bloom/helpers/` directory listings in full to match existing naming/structure conventions exactly (do not invent a new pattern shape for this one component).
3. Build `options-table-editor.vue` with the four consumers' actual column configs as the acceptance test (all four must render pixel-identical Bootstrap-classed markup to their current X-Templates - this is a mechanical port, not a redesign).
4. Build the modal sub-part (JSON textarea fallback editor) as either a second SFC or an internal sub-component of the same file - match whatever `media-picker`/`media-gallery` already do for similar "table + edit modal" UI if a precedent exists there (check before inventing a new modal-composition pattern).
5. Wire the 4 consumers' `.ts` entries to call the shared mount helper with their specific `columns`/`defaultColumn`/`addLabel` config, each remaining otherwise as thin as `options-editor-fields.ts`/`openid-client-settings.ts`/`seo-meta-part.ts` already are today.
6. Extract each of the 4 views' X-Templates, leaving just the mount-point div + its `data-*` attributes (column config can be passed as a JSON `data-columns` attribute, or hard-coded per consumer's `.ts` entry - prefer hard-coding in the `.ts` entry since column config is static per-consumer, not server-driven, avoiding a redundant server-to-client JSON round-trip for static config).
7. Delete the now-fully-replaced `optionsEditor.js`/`parametersEditor.js`/`customMetaTagsEditor.js` files and their `Assets.json` entries.
8. Set up ONE shared build entry for `options-table-editor.vue` if bloom components build once and get imported by consumers (check how `sortable-menu.ts`/`evalScripts.ts` are built/consumed today - likely already resolved by the existing shared-bloom-helpers pattern, no new build wiring needed beyond a normal TS import).
9. Build, smoke-test all 4 admin views manually (TextField predefined-list settings, MultiTextField settings, OpenId client parameters, Seo custom meta tags) - table add/remove/reorder, default-marking (checkbox per the decision), JSON-modal round-trip, and for Seo specifically the AJAX-widget-injection case.
10. Run the full `*Cms*` functional suite plus the new Seo AJAX-injection test.
11. Commit: one commit for the shared component itself, one commit per consumer migration (or squash if the reviewer prefers - ask).

### Task 4: Migrate the vue-draggable-dependent piece of the shared component (per Task 1's decision)

**Objective:** The new `options-table-editor.vue` built in Task 3 still needs SOME reorder mechanism for its draggable rows - resolve this using Task 1's decision (vuedraggable@next vs. hand-rolled SortableJS) rather than deciding it independently mid-Task-3. If Task 1 hasn't run yet, run it first.

**Files:** (already covered by Task 3's file list - this task is really "Task 3's reorder-mechanism sub-step," called out separately only because it depends on Task 1's decision, which may land later)

**Steps:**
1. Confirm Task 1's decision is recorded before finishing Task 3 step 3 (building the table).
2. Wire whichever reorder mechanism was chosen into `options-table-editor.vue`'s row list.
3. No separate build/test cycle beyond what Task 3 already covers - this is folded into Task 3's verification.

### Task 5: Migrate the vue-multiselect group (6 consumers)

**Objective:** Swap `vue-multiselect@2.1.6` → `@3.2.0` (already vendored) across all 6 consumers in one coherent effort, since they share the exact same multiselect-picker shape.

**Files:**
- `src/OrchardCore.Modules/OrchardCore.ContentFields/Assets/js/vue-multiselect-userpicker.js` (also uses vue-draggable + Sortable - coordinate with Task 3 if not already done)
- `src/OrchardCore.Modules/OrchardCore.ContentFields/Assets/js/vue-multiselect-multitextfieldpicker.js`
- `src/OrchardCore.Modules/OrchardCore.Menu/Assets/js/menu-permission-picker.js` (already has a co-located `menu-item-permission-part.ts` ES-module wrapper - fold the Vue app into that file directly rather than keeping two files)
- `src/OrchardCore.Modules/OrchardCore.AdminMenu/Assets/js/admin-menu-permission-picker.js` (near-duplicate of the Menu one above - extract one genuinely shared component this time, used by both modules, rather than two copies)
- `src/OrchardCore.Modules/OrchardCore.Taxonomies/Assets/js/tags-editor.js`
- `src/OrchardCore.Modules/OrchardCore.Shortcodes/Assets/js/shortcode-templates.js`
- `src/OrchardCore.Modules/OrchardCore.Resources/Assets/js/vue-multiselect-wrapper.js` (generic wrapper - check all `initVueMultiselect(` call sites via `search_files` before touching; this may be consumed by more views than the 6 explicitly listed)

**Steps:**
1. Before any code change: run `search_files(pattern="initVueMultiselect\\(", path="src")` and `search_files(pattern="depends-on=.*vue-multiselect-wrapper", path="src")` to get the COMPLETE consumer list for the generic wrapper (row 16) - the inventory above is from a first pass and may be incomplete for this specific file.
2. Read `vue-multiselect@3.2.0`'s actual installed source/types (`node_modules/vue-multiselect` after `yarn install`, or its published CHANGELOG) to confirm the prop/event API surface vs. 2.1.6 before assuming any file is a drop-in swap.
3. For each of the 6 (or more, per step 1) consumers: extract X-Template → SFC, swap `Vue.component('vue-multiselect', window.VueMultiselect.default)` (global-UMD registration pattern) for a real `import Multiselect from "vue-multiselect"` + local component registration, update `createApp`, update any renamed props/events found in step 2.
4. Two files (`menu-permission-picker.js`, `admin-menu-permission-picker.js`) are near-identical - extract a single shared component (candidate location: a new `.scripts/bloom/components/permission-picker.vue` or similar, following the existing shared-component precedent of `sortable-menu.ts`/`evalScripts.ts`) consumed by both modules' thin wrapper `.ts` files, rather than perpetuating the duplication into Vue 3.
5. Build, smoke-test each of the (at least) 6 admin views manually - permission pickers, tags editor, shortcode-template modal's multiselect, multitextfield picker, userpicker.
6. Run the full `*Cms*` functional suite (same command as Task 2 step 8).
7. Commit per logical group (the shared permission-picker extraction as one commit, each remaining standalone consumer as its own commit).

### Task 6: Migrate the remaining standalone consumers (Cors, Localization, Shortcodes/shortcodes.js)

**Objective:** Clean up the 3 remaining call sites that use neither vue-draggable nor vue-multiselect. (Localization's `optionsEditor.js` is the structurally-different culture-picker widget flagged in Task 3 - not part of the shared OptionsTableEditor.)

**Files:**
- `src/OrchardCore.Modules/OrchardCore.Cors/Assets/Admin/cors-admin.js` (uses `Vue.component` global registration for 2 sub-components - both convert to real SFCs)
- `src/OrchardCore.Modules/OrchardCore.Localization/Assets/js/optionsEditor.js`
- `src/OrchardCore.Modules/OrchardCore.Shortcodes/Assets/js/shortcodes.js`

**Steps:**
1. Same extraction pattern as Tasks 3/5: X-Template → SFC, `Vue.component`/`new Vue` → `createApp`.
2. Cors specifically: `window.corsApp` is read/written by name elsewhere per the existing PR #19489 description ("`corsApp` promoted to `window.corsApp` so the new TS entry `cors-admin-index.ts` can reach it") - check `cors-admin-index.ts` for exactly how it's consumed and preserve that integration point (likely still exposing the mounted app instance on `window.corsApp`, just now a Vue 3 app instance instead of Vue 2).
3. Build, smoke-test each of the 3 views.
4. Run the full `*Cms*` functional suite.
5. Commit per file.

### Task 7: Retire the global Vue 2 / vue-multiselect 2.x / vue-draggable resources

**Objective:** Now that section B's 17 call sites are all migrated, remove the now-dead global resource definitions and vendor files.

**Files:**
- `src/OrchardCore.Modules/OrchardCore.Resources/ResourceManagementOptionsConfiguration.cs` (remove the `vuejs:2` DefineScript block ~1127-1141, the `vue-multiselect:2` DefineScript block ~1159-1169, the `vue-draggable` DefineScript block ~1237-1250 - ONLY if Task 1 decided against keeping any vue-draggable-based replacement)
- `src/OrchardCore.Modules/OrchardCore.Resources/Assets.json` (remove matching vendor-copy entries for `vue-2.7.16`, `vue-multiselect-2.1.6`, `vue-draggable-2.24.3`)
- Delete: `src/OrchardCore.Modules/OrchardCore.Resources/wwwroot/Vendor/vue-2.7.16/`, `vue-multiselect-2.1.6/`, `vue-draggable-2.24.3/` (generated/copied output - regenerate via `yarn build` rather than hand-deleting if these are build artifacts checked into source; verify via `git log --follow` on one file first)

**Steps:**
1. Grep the WHOLE repo one more time for `vuejs:2`, `vue-multiselect:2`, `depends-on=".*vue-draggable` to confirm truly zero remaining references (Tasks 2-5 should have eliminated all of them, but this is the safety check before deleting shared infra).
2. Remove the DefineScript blocks and Assets.json vendor-copy entries.
3. `yarn build` (full, unscoped) to regenerate the resources bundle and confirm no build step still references the deleted vendor paths.
4. `git status --short | grep -v "^??"` - expect zero unexpected drift beyond the intended deletions.
5. Full solution build + `yarn lint && yarn check` + full `*Cms*` functional suite + `dotnet test test/OrchardCore.Tests/OrchardCore.Tests.csproj` (unit tests) - all green (allow the known pre-existing Lucene-lock-contention unit-test flake and the two known local Playwright flakes, same as every other verification pass this session).
6. Commit: `git commit -m "Remove now-unused Vue 2 / vue-multiselect 2.x / vue-draggable global resources"`.

### Task 8: Final verification and PR

**Objective:** Confirm the whole branch is coherent before opening the PR.

**Steps:**
1. `dotnet build OrchardCore.slnx -c Release -p:TreatWarningsAsErrors=true --warnaserror -p:RunAnalyzers=true -p:NuGetAudit=false` - clean.
2. `corepack enable && yarn install --immutable && yarn build` (full, unscoped) - `git status --short | grep -v "^??"` shows zero drift.
3. `yarn lint && yarn check` - clean.
4. `dotnet test test/OrchardCore.Tests.Functional/OrchardCore.Tests.Functional.csproj --filter-class "*Cms*"` - green (modulo the two known local flakes).
5. `dotnet test test/OrchardCore.Tests/OrchardCore.Tests.csproj` - green (modulo the known Lucene flake).
6. `git push origin skrypt/vue3-migration` - per this session's established pattern, the assistant pushes the branch; the user opens the PR manually on GitHub (per their stated preference: "I created them manually on Github thanks").
7. Draft a PR description summarizing: what moved (13 migration units covering the original 17 call sites, full inventory table from this plan, noting the 4-into-1 OptionsTableEditor unification), what got deleted (Vue 2 UMD, vue-multiselect 2.x, vue-draggable, the stale AdminMenu Vue 2 npm dependency, 4 duplicate options-editor files replaced by 1 shared component), what changed API-wise (Options API → mix of Options/Composition per file, vue-multiselect 2→3 prop/event renames if any were found in Task 5 step 2), and a link back to this plan file for full task-by-task detail.

---

## Files Likely to Change (summary)

- 13 `.js`/`.ts` Vue-instance files → converted to `.vue` SFCs + thin TS mount entries: 4 unify into the shared `options-table-editor.vue` (Task 3), the remaining 9 stay one-to-one (see inventory table for exact paths).
- 1 new shared component: `.scripts/bloom/components/options-table-editor.vue` + its mount helper (Task 3), replacing 4 near-duplicate files outright.
- ~14 `.cshtml` views (X-Templates extracted, `depends-on="vuejs:2"` etc. attributes updated/removed).
- A handful of `Assets.json` files gaining new `"action": "vite"` mini-app entries (or `"action": "parcel"` for the single-file `content-type-picker.ts` case) - fewer than a strict 1:1 count since the 4 OptionsTableEditor consumers share one build entry instead of 4.
- New `package.json` per Vite mini-app (following `media-picker`/`media-gallery`'s exact shape).
- `ResourceManagementOptionsConfiguration.cs` - remove 3 DefineScript blocks (Task 7 only).
- `OrchardCore.Resources/Assets.json` + `wwwroot/Vendor/` - remove 3 vendor trees (Task 7 only).
- `OrchardCore.AdminMenu/Assets/package.json` - remove stale `vue` dependency (Task 0 only).

## Tests / Validation (per task, repeated at the end)

- `dotnet build OrchardCore.slnx -c Release -p:TreatWarningsAsErrors=true --warnaserror -p:RunAnalyzers=true -p:NuGetAudit=false`
- `yarn lint && yarn check`
- `yarn build` (full) + `git status --short | grep -v "^??"` for zero bundle drift
- `dotnet test test/OrchardCore.Tests.Functional/OrchardCore.Tests.Functional.csproj --filter-class "*Cms*"`
- `dotnet test test/OrchardCore.Tests/OrchardCore.Tests.csproj`
- Manual smoke-test of every migrated admin view via the `orchardcore-tester` skill (Playwright automation) before considering a task done - visual/interactive parity matters here since this is a framework-API rewrite with no intended UX change.

## Risks, Tradeoffs, and Open Questions

- **vue-draggable replacement direction (Task 1)** is a genuine open decision, not a foregone conclusion - flag it to the user explicitly rather than silently picking one. `vuedraggable@next` is less work per-file but keeps a dependency; hand-rolled SortableJS matches this repo's already-established direction (see the jQuery-UI-removal PR's Menu/Taxonomies/Flows/Widgets/Layers/AdminDashboard precedent) but is more work upfront for a payoff of one fewer dependency. This decision now gates Task 4 (the shared OptionsTableEditor's own reorder mechanism) specifically, not a scattered set of 5 independent files as originally scoped - one decision, one implementation site.
- **AJAX-reachability is the single highest-risk area** (Forms/SelectPart, Seo/SeoMetaPart) - this exact bug class (editor works on full page load, silently fails to init when injected via AJAX) has already shipped once in this codebase's history this session (the Features admin page regression in PR #19522/#19489, caught only because it was investigated manually) - do not skip the `observeAndInit`-style wrapping or the AJAX-injection functional test for these two. Forms/SelectPart is migrated in Task 5 (multiselect group); Seo/SeoMetaPart is migrated in Task 3 (shared OptionsTableEditor) - both tasks call this out explicitly.
- **The OptionsTableEditor unification (Task 3) has one open sub-decision of its own**: ContentFields' two current consumers disagree on "default" selection UI (radio vs. per-row checkbox) - flagged for explicit user confirmation before implementation, not silently resolved.
- **vue-multiselect 2.x → 3.2.0 API drift is unverified** - this plan explicitly calls out reading the real installed package's types/CHANGELOG (Task 5 step 2) rather than assuming parity; do not proceed past that step on assumption.
- **Duplication cleanup (Menu vs AdminMenu permission pickers) is a scope-creep risk** - Task 5 step 4 proposes extracting a shared component while migrating, which is more work than a literal 1:1 port of each file. This is flagged as a judgement call, not a hard requirement - a literal 1:1 port-times-two is an acceptable, lower-risk alternative if time-boxed. (This mirrors the OptionsTableEditor unification in Task 3, but is kept as a separate, smaller, optional decision rather than bundled into the same task - the permission-picker pair is 2 files, not 4, and lower-risk to leave unmerged if time-boxed.)
- **Whether to also switch Options API → Composition API `<script setup>`** during this migration, vs. doing a more mechanical Options-API-preserving port first: this plan defaults to "whatever is lower-risk per file" (Task 2 explicitly allows staying Options API) rather than mandating `<script setup>` everywhere - `media-picker`/`media-gallery` already established `<script setup>` as the repo's preferred *new-code* style, but a mechanical Options-API port is a smaller, more reviewable diff for *migrated* code. The new shared `options-table-editor.vue` (Task 3) IS a good candidate for `<script setup>` from the start, though, since it's genuinely new code (a merge of 4 old files), not a straight port of any single one of them - lean toward `<script setup>` there specifically.
- **Scale**: originally 17 call sites, now effectively ~13 independent migration units after the Task 3 unification, across 6+ modules, each needing its own build wiring - realistically several weeks of incremental work at the pace this session's ES-module/jQuery-removal PRs (#19522/#19489, 65 commits, 1804 files) actually took. This plan is deliberately structured as 8 independently-mergeable/revertable tasks rather than one PR, learning directly from the "45-commit divergence, 33 conflicting files on the first rebase commit" pain already hit once this session when a large branch drifted too far from its base before merging.

