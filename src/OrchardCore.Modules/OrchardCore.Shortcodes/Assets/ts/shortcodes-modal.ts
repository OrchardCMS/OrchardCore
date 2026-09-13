// Defined by this module's own Assets/js/shortcodes.js (classic global). ShortcodeModal.cshtml
// is only ever rendered once per page (guarded server-side via Context.Items), so this always
// finds the modal it just rendered.
declare function initializeShortcodesApp(element: Element | null): void;

initializeShortcodesApp(document.getElementById("shortcodeModal"));
