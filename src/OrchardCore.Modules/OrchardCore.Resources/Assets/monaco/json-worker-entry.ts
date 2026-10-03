// monaco-editor's FileAccess.toUri() (vs/base/common/network.js) falls back to
// `globalThis._VSCODE_FILE_ROOT` when resolving a worker's own module URL for its internal
// `$loadForeignModule` RPC call - if unset, it crashes trying to call `.toUrl()` on `undefined`.
// This only needs to be set inside the worker's own global scope (`self`), not the main thread's
// `window` - workers don't share globals with the page that spawned them. The value only needs to
// satisfy toUri()'s URL-join logic; the module it "resolves" is never actually fetched, since this
// worker file already bundles the JSON language service directly.
(self as unknown as { _VSCODE_FILE_ROOT?: string })._VSCODE_FILE_ROOT = self.location.href;

import "monaco-editor/esm/vs/language/json/json.worker.js";
