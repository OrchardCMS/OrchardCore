# Media Gallery Refactor Plan

## Background

The media gallery system is split across two Vue 3 applications and several Parcel-bundled TypeScript
editor plugins. Over time the split has introduced redundancy: a `MediaPickerModal` component exists
in both apps doing essentially the same job, `openMediaPicker` lives in the wrong bundle, and the
trumbowyg source files are duplicated verbatim across two modules.

This document records the agreed target architecture and the ordered implementation steps.

---

## Goals

1. **Single responsibility per bundle** — `media-gallery` owns the file library browser; `media-picker`
   (renamed from `media-field`) owns all picker/modal/field-editor concerns.
2. **Eliminate the duplicate `MediaPickerModal`** — one component, one bundle.
3. **Remove `openMediaPicker` from `media-gallery`** — it does not belong in the library browser.
4. **Lazy-load `media-gallery`** — the full browser bundle (~3 MB) is only fetched when the user
   actually opens the picker, not on every page that contains an HTML/Markdown field.
5. **Deduplicate the trumbowyg source files** — `OrchardCore.ContentFields` and `OrchardCore.Html`
   each carry identical copies; they should share a single canonical source.
6. **Favor pure ES module exports** — no new `window.*` globals. Cross-bundle communication uses
   the browser's ES module cache via `import()`, not window properties.
7. **Tailwind only** — both apps must use only Tailwind utility classes. Custom CSS namespaces
   (`ma-*` in `media-gallery`, `mf-*` in `media-picker`) are converted to Tailwind and their
   corresponding stylesheets removed. No Bootstrap classes anywhere.

---

## Current Architecture

```
Razor wrapper views
  └── loads media-gallery script eagerly  (media2.js, ~3 MB)

trumbowyg / MDE editor plugins  (Parcel bundles)
  └── import picker-api.ts  (from orchardcore-media-gallery workspace package)
        └── delegates to  window.OrchardCoreMedia.openMediaPicker()
              └── media-gallery/main.ts: openMediaPicker()
                    └── mounts MediaPickerModal.vue  (Bootstrap, lives in media-gallery)
                          └── calls mountMediaAppAsPicker()  (also in media-gallery)

media-field app  (orchardcore-media-field → media-field2.js)
  └── MediaFieldBasic / Gallery components
        └── MediaPickerModal.vue  (Tailwind, lives in media-field)
              └── dynamic import("@media-gallery") → mountMediaAppAsPicker()
```

**Problems:**

- `MediaPickerModal` exists twice with different styling systems.
- `openMediaPicker` is in `media-gallery` but conceptually belongs with field/picker concerns.
- Every Razor page with an HTML/Markdown editor pre-fetches the full 3 MB `media-gallery` bundle even
  if the picker is never opened.
- `trumbowyg.media.tag.ts` and `trumbowyg.media.url.ts` are copied verbatim into both
  `OrchardCore.ContentFields` and `OrchardCore.Html`.

---

## Target Architecture

```
Razor wrapper views
  └── loads media-picker script eagerly  (media-picker2.js, type="module")
  └── keeps media-gallery stylesheet eagerly  (media2.css, needed for picker UI)
  └── data-media-picker-config div gains data-picker-src attribute (absolute URL of media-picker2.js)

trumbowyg / MDE editor plugins  (Parcel bundles)
  └── import picker-api.ts  (from orchardcore-media-gallery workspace — path unchanged)
        └── reads data-picker-src from DOM
        └── await import(pickerSrc)  → browser returns cached module-picker2.js instance
              └── calls module.openMediaPicker(config)
                    └── media-picker/main.ts: openMediaPicker()  (named ESM export, no window)
                          └── mounts MediaPickerModal.vue  (single, Tailwind, lives in media-picker)
                                └── dynamic import("@media-gallery") → mountMediaAppAsPicker()

media-picker app  (orchardcore-media-picker → media-picker2.js, type="module")
  └── MediaFieldBasic / Gallery / Attached  (unchanged field editors)
  └── MediaPickerModal.vue  (the one true picker modal)
  └── openMediaPicker()  exported as a named ES module export — no window global
```

`media-gallery` retains:
- The full file library browser (`App.vue`, router, Globals, etc.)
- `mountMediaAppAsPicker()` — named ESM export, consumed only by `media-picker` via dynamic import

`media-gallery` loses:
- `MediaPickerModal.vue`
- `openMediaPicker()` function
- `window.OrchardCoreMedia` global — removed entirely, no replacement

---

## Key Design Decisions

### 1. `openMediaPicker` as a named ESM export in `media-picker` — no window global

Both Vite bundles (`media2.js` and `media-field2.js`) are already served with `type="module"` via
`.SetAttribute("type", "module")` in the C# resource manifest. The browser's ES module cache
therefore applies: a second `import('/path/to/media-picker2.js')` anywhere on the same page returns
the already-evaluated module instance at zero cost.

`media-picker/main.ts` exports `openMediaPicker` as a plain named export:

```ts
// media-picker/src/main.ts  (new addition — no window assignment)
export function openMediaPicker(config: IMediaPickerConfig): Promise<IMediaFieldItem[]> {
    return new Promise((resolve) => {
        const host = document.createElement('div');
        document.body.appendChild(host);

        const app = createFieldApp(MediaPickerModal, {
            ...configAsProps(config),
            autoOpen: true,
            onResolve: (items: IMediaFieldItem[]) => {
                app.unmount();
                host.remove();
                resolve(items);
            },
        });

        app.mount(host);
    });
}
// No window.OrchardCoreMediaPicker assignment
```

`MediaPickerModal.vue` gains an `autoOpen?: boolean` prop. When `true`, `onMounted` calls `open()`
so the programmatic path works without a component ref.

The return type is `IMediaFieldItem[]` (has `mediaPath`). The trumbowyg/MDE scripts already map the
result to their own types (`filePath`, `url`, `name`) — the mapping stays in those scripts.

### 2. `picker-api.ts` uses a dynamic import via `data-picker-src` — no window delegation

The trumbowyg/MDE Parcel packages declare `orchardcore-media-gallery` as a workspace dependency and
import from `orchardcore-media-gallery/src/picker-api`. The package reference stays unchanged.
`picker-api.ts` is rewritten to use a dynamic ES module import instead of a window lookup:

```ts
// media-gallery/src/picker-api.ts  (rewritten — no window)
type PickerModule = {
    openMediaPicker?: (config: MediaPickerConfig) => Promise<MediaPickerFile[]>;
};

export async function openMediaPicker(config: MediaPickerConfig): Promise<MediaPickerFile[]> {
    const el = document.querySelector<HTMLElement>('[data-media-picker-config]');
    const pickerSrc = el?.dataset.pickerSrc;
    if (!pickerSrc) return [];

    const mod = await import(/* @vite-ignore */ pickerSrc) as PickerModule;
    return mod.openMediaPicker?.(config) ?? [];
}
```

`data-picker-src` is added by the Razor wrapper views (see Phase 5). Because `media-picker2.js` is
already loaded as `type="module"` by the time the picker button is clicked, `import(pickerSrc)`
returns the cached instance synchronously from the module registry — no extra network request.

`MediaPickerFile` in `picker-api.ts` must remain a union covering `filePath`, `url`, and `name`
since the two trumbowyg plugins each read different fields.

### 3. Lazy loading `media-gallery`

`media-picker`'s `MediaPickerModal` already uses `await import('@media-gallery')` inside `onOpened`,
which fires only when the modal is visible. Vite externalises `@media-gallery` → `./media2.js` in the
rollup output, so the dynamic import in the final bundle is `import('./media2.js')`.

This means `media2.js` is never fetched until the picker opens. The Razor wrapper views can drop
`<script asp-name="media">` and replace it with `<script asp-name="media-picker">`.

The `media2.css` stylesheet must still be loaded eagerly because the browser cannot inject CSS
synchronously at the moment the modal opens. Keep `<style asp-name="media" at="Head">` in the
wrapper views.

### 4. Trumbowyg source deduplication

`OrchardCore.ContentFields` and `OrchardCore.Html` both compile
`trumbowyg.media.tag.ts` and `trumbowyg.media.url.ts` with Parcel. The source files are now
identical (after the `data-media-picker-config` refactor). They should be moved into the
`orchardcore-media-gallery` package under `src/trumbowyg/` and re-exported, so both modules import
from the shared workspace package and Parcel bundles the canonical source.

Each module still produces its own wwwroot output and registers its own named resources — this step
is purely a source-level deduplication.

### 5. `window.OrchardCoreMedia` is removed entirely

After the migration `mountMediaAppAsPicker` is consumed only by `media-picker`'s dynamic import
(`await import("@media-gallery")`), which resolves to the ES module — no window property needed.
`window.OrchardCoreMedia` is removed with no replacement. Both old properties
(`openMediaPicker` and `mountMediaAppAsPicker`) disappear from the global scope.

---

## Implementation Steps

### Phase 1 — Rename `media-field` → `media-picker`

**Files changed:**

| Action | Path |
|--------|------|
| Rename directory | `Assets/media-field/` → `Assets/media-picker/` |
| Update `package.json` | `"name": "orchardcore-media-picker"` |
| Update `vite.config.ts` | output filenames `media-picker2.js` / `media-picker2.css` |
| Update `Assets.json` | rename source entry from `media-field` to `media-picker` |
| Update `ResourceManagementOptionsConfiguration.cs` | script/style paths `Scripts/media-picker2.js`, `Styles/media-picker2.css` |
| Update Razor views | any `asp-name="media-field"` references |
| Update root `package.json` workspaces | if the directory rename is not picked up automatically |

No logic changes in this phase — it is a pure rename/move.

### Phase 2 — Add `openMediaPicker` and `autoOpen` prop to `media-picker`

1. Add `autoOpen?: boolean` prop to `MediaPickerModal.vue`. On `onMounted`, if `autoOpen` is true,
   call `open()`.
2. Add `onResolve` prop to `MediaPickerModal.vue` for the programmatic case. When provided, call it
   instead of (or in addition to) emitting `select`, then close.
3. Add `openMediaPicker(config)` factory function to `media-picker/src/main.ts` as a **named ES
   module export only** — no `window.*` assignment.

### Phase 3 — Rewrite `picker-api.ts` in `media-gallery`

Replace the window-lookup delegation with a pure ESM dynamic import:

1. Remove `MediaPickerRuntimeWindow` and the window cast entirely.
2. Read `data-picker-src` from the `[data-media-picker-config]` element.
3. `await import(pickerSrc)` to obtain the `media-picker` module and call its `openMediaPicker`.
4. Ensure `MediaPickerFile` type covers `filePath`, `url`, and `name` as optional fields.

### Phase 4 — Remove picker concerns from `media-gallery`

1. Delete `media-gallery/src/components/MediaPickerModal.vue`.
2. Remove the `openMediaPicker` function and its imports from `media-gallery/src/main.ts`.
3. Remove `window.OrchardCoreMedia` global declaration and assignment.
4. Keep `mountMediaAppAsPicker` as a named ESM export — do not put it on window.

### Phase 5 — Update Razor wrapper views

In all three wrapper views:

| Module | View |
|--------|------|
| `OrchardCore.ContentFields` | `Media-HtmlField.Wrapper.cshtml` |
| `OrchardCore.Html` | `Media-HtmlBodyPart.Wrapper.cshtml` |
| `OrchardCore.Markdown` | `Media-MarkdownBodyPart.Wrapper.cshtml` |

1. Replace `<script asp-name="media">` with `<script asp-name="media-picker">`.
2. Keep `<style asp-name="media" at="Head">` — the media-gallery stylesheet must remain eagerly loaded
   because CSS cannot be injected synchronously at modal-open time.
3. Add `data-picker-src` to the existing `data-media-picker-config` div so `picker-api.ts` can
   resolve the module URL without hardcoding it:

```cshtml
<div hidden
     data-media-picker-config
     data-translations="@JsonSerializer.Serialize(jsLocalizations)"
     data-base-path="@Url.Content("~/")"
     data-upload-files-url="@Url.Content("~/api/media/Upload")"
     data-picker-src="@Url.Content("~/OrchardCore.Media/Scripts/media-picker2.js")"></div>
```

Register `media-picker` (script + style) in `ResourceManifestOptionsConfiguration.cs` in the
Media module with `SetAttribute("type", "module")` — consistent with `media` and `media-field`.

### Phase 6 — Deduplicate trumbowyg source files

1. Move `trumbowyg.media.tag.ts` and `trumbowyg.media.url.ts` into
   `media-gallery/src/trumbowyg/` (they live in `orchardcore-media-gallery` which is already a dep of both
   consuming modules).
2. In `OrchardCore.ContentFields/Assets/js/` and `OrchardCore.Html/Assets/`, replace the local
   `.ts` files with thin re-exports:
   ```ts
   export { } from 'orchardcore-media-gallery/src/trumbowyg/trumbowyg.media.tag';
   ```
3. Parcel resolves the workspace import and bundles the canonical source into each module's wwwroot
   output. No change to resource registrations or wwwroot paths.

### Phase 7 — Convert to Tailwind only

Both apps currently use scoped custom CSS namespaces alongside Tailwind. The goal is to eliminate
those separate stylesheets and express all styling as inline Tailwind utilities.

**`media-gallery` (`ma-*` classes)**

`media-gallery` uses a hand-written CSS file alongside the Vite build. All `ma-btn`, `ma-input`,
`ma-alert`, `ma-btn-group`, and layout classes across `App.vue` and every component must be
replaced with Tailwind equivalents. The separate CSS asset is removed from the build output.

**`media-picker` (`mf-*` classes)**

`media-picker` (renamed from `media-field`) uses `field.css` with `mf-*` classes. The same
conversion applies: replace every `mf-*` reference in Vue components with Tailwind utilities and
delete `field.css`.

**Approach**

Convert one component at a time, working from leaf components upward (e.g. buttons and inputs
before containers and modals). Use the `tw:` prefix consistently for all Tailwind classes to
preserve future compatibility with Tailwind's PostCSS mode.

After conversion, remove the CSS asset entries from each app's `vite.config.ts` and confirm that
the built output no longer includes a separate stylesheet for either app.

---

## Files Affected (full list)

### `OrchardCore.Media`

- `Assets/media-field/` → `Assets/media-picker/` (entire directory rename)
- `Assets/media-picker/package.json` — package name
- `Assets/media-picker/vite.config.ts` — output paths
- `Assets/media-picker/src/main.ts` — add `openMediaPicker` as named ESM export (no window global)
- `Assets/media-picker/src/components/MediaPickerModal.vue` — add `autoOpen` + `onResolve` props
- `Assets/media-gallery/src/picker-api.ts` — rewrite to use `await import(data-picker-src)`, remove window references
- `Assets/media-gallery/src/main.ts` — remove `openMediaPicker`, remove `window.OrchardCoreMedia` entirely
- `Assets/media-gallery/src/components/MediaPickerModal.vue` — delete
- `Assets/media-gallery/src/trumbowyg/trumbowyg.media.tag.ts` — new canonical source (moved)
- `Assets/media-gallery/src/trumbowyg/trumbowyg.media.url.ts` — new canonical source (moved)
- `Assets/media-gallery/src/**/*.vue` — convert `ma-*` custom classes to Tailwind `tw:` utilities
- `Assets/media-gallery/src/assets/` — delete custom CSS file (or equivalent) after conversion
- `Assets/media-picker/src/**/*.vue` — convert `mf-*` custom classes to Tailwind `tw:` utilities
- `Assets/media-picker/src/assets/field.css` — delete after conversion
- `Assets/media-picker/vite.config.ts` — remove CSS asset output entry after stylesheet removal
- `Assets.json` — rename `media-field` entry to `media-picker`
- `ResourceManagementOptionsConfiguration.cs` — add `media-picker` resource, update paths; remove
  stylesheet registrations for both apps once CSS files are eliminated

### `OrchardCore.ContentFields`

- `Assets/js/trumbowyg.media.tag.ts` — becomes re-export of shared source
- `Assets/js/trumbowyg.media.url.ts` — becomes re-export of shared source
- `Views/Media-HtmlField.Wrapper.cshtml` — swap `media` script for `media-picker`, add `data-picker-src`

### `OrchardCore.Html`

- `Assets/trumbowyg.media.tag.ts` — becomes re-export of shared source
- `Assets/trumbowyg.media.url.ts` — becomes re-export of shared source
- `Views/Media-HtmlBodyPart.Wrapper.cshtml` — swap `media` script for `media-picker`, add `data-picker-src`

### `OrchardCore.Markdown`

- `Assets/Scripts/mde.mediatoolbar.ts` — no source change; `picker-api.ts` update is transparent
- `Views/Media-MarkdownBodyPart.Wrapper.cshtml` — swap `media` script for `media-picker`, add `data-picker-src`

---

## Out of Scope

- Merging the trumbowyg wwwroot outputs into a single shared resource owned by the Media module.
  The source deduplication in Phase 6 is sufficient; the two-module resource split is an
  OrchardCore module boundary concern.
- Changing how `media-gallery` is bundled (Vue Router, Globals, etc.) — that is out of scope.
- Any changes to the `OrchardCore.Media` C# backend.
