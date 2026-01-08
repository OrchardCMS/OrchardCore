// Workaround for Monaco Editor bug with browser autocomplete events
// This fixes the "getModifierState is not a function" error that occurs
// when browser autocomplete is used in fields on pages with Monaco Editor.
// See: https://github.com/microsoft/monaco-editor/issues/4325
(function() {
    document.addEventListener('keydown', function(e) {
        if (e && typeof e.getModifierState !== 'function') {
            e.getModifierState = function() { return false; };
        }
    }, true);
})();
