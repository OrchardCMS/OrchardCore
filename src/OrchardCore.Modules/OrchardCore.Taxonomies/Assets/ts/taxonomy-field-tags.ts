import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

// Defined by this module's own Assets/js/tags-editor.js (classic global).
declare function initializeTagsEditor(element: Element | null): void;

observeAndInit(".tags", (element) => initializeTagsEditor(element));
