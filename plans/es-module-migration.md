# ES module migration for dynamically-injected field/part editor scripts

## Context

PR #19489 (large jQuery-removal refactor, branch `skrypt/yarn-check`) migrated inline Razor view scripts from jQuery `$(function(){...})` to `document.addEventListener('DOMContentLoaded', ...)`. That broke Flow/Bag/Widgets-list editors, which are injected into the DOM via AJAX well after `DOMContentLoaded` already fired — a bare listener never fires for them. PR #19442 (merged) fixed the pre-existing 89 views with a local `onDocumentReady` helper that runs its callback immediately if the document is already parsed, otherwise waits for `DOMContentLoaded`. On `skrypt/yarn-check` we applied the same wrapper to 9 more files PR #19489 introduced with the same bug.

The PR author then asked: what if these views used real ES modules (`type="module"`) instead of the helper? Modules never listen for `DOMContentLoaded` at all — they run once parsed, which lands after the document is ready on a normal page load and immediately when injected later — so the whole `onDocumentReady` workaround becomes unnecessary. They also give real lexical scoping, removing any risk of `var`/function name collisions between views rendered together on one page (e.g. two instances of the same field on a Flow/Bag).

This branch (`skrypt/es-module`, based on `skrypt/yarn-check`) is where that idea gets scoped and implemented. Verified findings below (via two research passes plus spot-checks) determine the scope — it deliberately does **not** touch every `at="Foot"` script in the repo, only the ones that are actually reachable through AJAX fragment injection.

## Verified findings

**Server side needs no changes.** `ScriptTagHelper.ProcessCustomContentAsync` (`src/OrchardCore/OrchardCore.ResourceManagement/TagHelpers/ScriptTagHelper.cs`, ~174-223) copies every attribute on a plain `<script at="Foot">` — including an unbound `type` — verbatim into the registered `TagBuilder`. `ResourceManager`'s `RenderFootScript`/`RenderHeadScript` render it unmodified (no hardcoded `type`, unlike stylesheets). `type="module"` already survives this pipeline today.

**Only 4 places in the repo dynamically inject an HTML fragment + execute its scripts:**
- `src/OrchardCore.Modules/OrchardCore.Flows/Assets/ts/flows.edit.ts:17-27` — `evalScripts()`, creates a real `<script>` via `createElement`, copies all attributes (including `type`), appends it. **Module-safe.** Governs FlowPart and BagPart editors.
- `src/OrchardCore.Modules/OrchardCore.Widgets/Assets/ts/widgetslist.edit.ts:16-27` — byte-identical `evalScripts()`. **Module-safe.** Governs the Widgets-list editor, and (confirmed via `WidgetsListPart.Edit.cshtml:124`) Layers' widget-zone UI too.
- `src/OrchardCore.Modules/OrchardCore.Flows/Assets/js/content-type-picker.js:165-176` — a third copy-pasted `evalScripts()`, used by the shared "Add" content-type-picker modal. **Module-safe as written**, but its `Assets.json` entry uses `"action": "min"` (bare minifier, not Parcel) and it's plain `.js`, so it can't `import` a shared helper without a build-config change.
- `src/OrchardCore.Modules/OrchardCore.Layers/wwwroot/Scripts/layers.edit.js:34,57` — real jQuery `$.globalEval`, **not module-safe** (evaluates script text, not a real `<script>` element). Confirmed via repo-wide grep: **zero references** anywhere (`.cshtml`, `.cs`, manifests) — this file is orphaned dead code, superseded by Widgets' `evalScripts` path. Not a live reachability concern.

No other AJAX fragment-injection-with-scripts pattern exists anywhere in the repo (workflow activity editors, deployment plan steps, AdminMenu tree nodes, content type/part/field settings editors, Rules, ContentPreview, Dynamic Forms all use full-page navigation or script-less AJAX).

**Scope implication for the 9 files already patched with `onDocumentReady`:** only the Content Field/Part editors reachable when attached to a FlowPart/BagPart/Widgets-list are actually reachable via AJAX injection:
- `HtmlField-Trumbowyg.Edit.cshtml`, `HtmlField-Wysiwyg.Edit.cshtml`, `TextField-IconPicker.Edit.cshtml` (`OrchardCore.ContentFields`)
- `HtmlBodyPart-Trumbowyg.Edit.cshtml`, `HtmlBodyPart-Wysiwyg.Edit.cshtml` (`OrchardCore.Html`)

`ContentTypes/Admin/Edit.cshtml`, `Layers/Admin/Index.cshtml`, and AdminMenu's `LinkAdminNode.Fields.TreeEdit.cshtml`/`PlaceholderAdminNode.Fields.TreeEdit.cshtml` are **not** reachable via AJAX injection (confirmed: full-page navigation only) — their `onDocumentReady` wrapper was precautionary consistency, not a live-bug fix, and converting them to modules would add risk for zero benefit. Leave them as-is.

**Shared frontend infra already supports this with no new tooling.** The yarn workspace includes `.scripts/bloom` as `@orchardcore/bloom`, and `flows.edit.ts` already does `import initSortableWidgets from "@orchardcore/bloom/components/sortable-widgets"` — confirmed. Existing `helpers/` files (`globals.ts`, `removeDiacritics.ts`) are plain named-export TS modules. Adding `evalScripts` there and importing it from both Flows and Widgets is a zero-new-build-config change.

## Plan

### Phase 1 — Pilot: `TextField-IconPicker.Edit.cshtml`

`src/OrchardCore.Modules/OrchardCore.ContentFields/Views/TextField-IconPicker.Edit.cshtml` — smallest, most self-contained of the candidates (no culture/RTL branching). Change `<script at="Foot">` to `<script at="Foot" type="module">` and delete the `onDocumentReady` wrapper, keeping only the callback body.

Pre-conversion safety check (repeat for every later file): no `window.`-assigned globals, no top-level `function`/`var` another script depends on, no strict-mode-incompatible constructs, only reads pre-existing globals (`$`/jQuery, the `iconpicker` plugin). Confirmed clean for this file. No CSP `nonce` in use anywhere in the repo currently (non-issue).

**Manual verification required before merging (do not skip, no automated test covers this):**
1. Full-page load: a content item edit page where this field is on the *type's own* editor (not nested in Flow/Bag). Check devtools — no console errors, script tag has `type="module"`, icon picker initializes and updates the hidden input.
2. AJAX dynamic-injection (the actual bug surface): add this field to a widget type used by a FlowPart/BagPart, open a content item with that part, click "Add" to trigger the AJAX `evalScripts` path. Check devtools — the injected `<script>` element has `type="module"`, no console errors, icon picker initializes on the fresh DOM node, and still works correctly with 2+ instances of the field on the page at once (the actual point of the scoping change).
3. Confirm the icon picker is usable immediately after the AJAX response resolves, not just "eventually" — dynamically-created module scripts execute deferred/async rather than synchronously, so this is the one timing behavior that's actually changing.

### Phase 2 — Widen to the remaining Mechanism-A-reachable views, one at a time, same two-path verification each

In order: `HtmlField-Wysiwyg.Edit.cshtml` → `HtmlField-Trumbowyg.Edit.cshtml` → `HtmlBodyPart-Wysiwyg.Edit.cshtml` → `HtmlBodyPart-Trumbowyg.Edit.cshtml`. These have culture-dependent branching (`@culture.TwoLetterISOLanguageName` spliced into `jQuery.trumbowyg.langs.@culture...`) and Razor-injected JSON (`@Html.Raw(trumbowygSettings.Options)`) — confirm the emitted JS is still valid under strict mode (should be identical text; spot-check a non-English locale).

**Once 2+ are converted, explicitly test mixing them**: a widget type combining two of the converted fields (e.g. Html-Trumbowyg + Icon Picker text field) added together in one Flow/Bag "Add" action. `evalScripts()`'s loop appends classic scripts synchronously but module scripts execute deferred — mixing converted and not-yet-converted scripts in one injected fragment loses relative-order guarantees between them. None of these views currently depend on cross-script ordering, but click through it in devtools before calling Phase 2 done.

### Phase 3 — Dedupe `evalScripts` (independent, can run alongside Phase 1/2)

1. Add `.scripts/bloom/helpers/evalScripts.ts`, following the existing `helpers/` style (plain named export), containing the function currently duplicated in `flows.edit.ts:17-27` and `widgetslist.edit.ts:16-27`. Keep the existing explanatory comment about why it exists.
2. Update both files to `import evalScripts from "@orchardcore/bloom/helpers/evalScripts";` and delete their local copies.
3. Leave `content-type-picker.js` alone here — dedup it only as an optional separate follow-up that also flips its `Assets.json` action from `"min"` to `"parcel"` and converts it to `.ts`. Call this out as optional/out-of-scope for this branch, since it's a build-tooling change, not a copy-paste fix.

### Explicitly out of scope for this branch

- `ContentTypes/Admin/Edit.cshtml`, `Layers/Admin/Index.cshtml`, both AdminMenu TreeEdit views — not reachable via AJAX injection, no benefit from converting, leave the `onDocumentReady` wrapper as-is.
- `layers.edit.js` — confirmed orphaned dead code (zero references repo-wide). Worth flagging as a separate cleanup PR/issue, but deleting it is unrelated churn — don't mix it into this branch.
- `content-type-picker.js`'s build-action change (see Phase 3.3).

## Verification summary

Every converted file needs the same manual two-path check in a running dev server (`yarn watch` + `dotnet run`): (a) normal full-page load of the field/part on its own content type editor, (b) AJAX-injected via a Flow/Bag/Widgets-list "Add" action, both checked in browser devtools for console errors, correct `type="module"` on the rendered/injected script tag, and correct initialization with no timing regressions. No existing automated test covers this path (it's pure client-side DOM/timing behavior), so this manual pass is the actual gate — call it out explicitly when each file's change is ready for review.
