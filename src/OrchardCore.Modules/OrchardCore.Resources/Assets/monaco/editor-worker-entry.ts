// See json-worker-entry.ts for why this is needed.
(self as unknown as { _VSCODE_FILE_ROOT?: string })._VSCODE_FILE_ROOT = self.location.href;

import "monaco-editor/esm/vs/editor/editor.worker.js";
